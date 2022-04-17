using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//           The text from LICENSE file:
//
//Copyright (c) 2019 Sebastian Lague
//
//Permission is hereby granted, free of charge, to any person obtaining a copy
//of this software and associated documentation files (the "Software"), to deal
//in the Software without restriction, including without limitation the rights
//to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//copies of the Software, and to permit persons to whom the Software is
//furnished to do so, subject to the following conditions:
//
//The above copyright notice and this permission notice shall be included in all
//copies or substantial portions of the Software.
//
//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//SOFTWARE.

public class CloudNoise : MonoBehaviour {

    public bool logComputeTime;

    [Header ("Base Noise")]
    public bool updateBaseNoise = true;
    public ComputeShader noiseCompute;
    public RenderTexture noiseTexture;
    public int baseResolution = 128;

    public Vector3 noiseTestParams;

    public SimplexSettings simplexSettings;
    public WorleySettings worleySettings;
    [HideInInspector]
    public bool calculateWorley = true;

    [Header ("Detail Noise")]
    public int detailNoiseResolution = 32;
    public RenderTexture detailNoiseTexture;
    public WorleySettings detailWorleySettings;

    bool updateNoise = true;
    List<ComputeBuffer> buffersToRelease;

    [ContextMenu ("Run")]
    public void UpdateNoise () {
        CreateTexture (ref noiseTexture, baseResolution);
        CreateTexture (ref detailNoiseTexture, detailNoiseResolution);

        if (updateNoise) {
            updateNoise = false;

            // Create stopwatches
            var mainTimer = System.Diagnostics.Stopwatch.StartNew ();
            var computeTimer = new System.Diagnostics.Stopwatch ();

            int threadGroupSize = 8;
            int numBaseNoiseThreadGroups = Mathf.CeilToInt (baseResolution / (float) threadGroupSize);
            int numDetailNoiseThreadGroups = Mathf.CeilToInt (detailNoiseResolution / (float) threadGroupSize);

            buffersToRelease = new List<ComputeBuffer> ();
            noiseCompute.SetVector ("testParams", noiseTestParams);
            noiseCompute.SetTexture (0, "Result", noiseTexture);
            var testBuffer = CreateBuffer (new int[numBaseNoiseThreadGroups], sizeof (int), "testBuffer");

            UpdateSimplex ();
            UpdateWorley (worleySettings);

            mainTimer.Stop ();
            computeTimer.Start ();

            // Dispatch base noise
            if (updateBaseNoise) {
                noiseCompute.Dispatch (0, numBaseNoiseThreadGroups, numBaseNoiseThreadGroups, numBaseNoiseThreadGroups);
            }

            // Dispatch detail noise
            UpdateWorley (detailWorleySettings);
            noiseCompute.SetInt ("resolution", detailNoiseResolution);
            noiseCompute.SetTexture (0, "Result", detailNoiseTexture);
            noiseCompute.Dispatch (0, numDetailNoiseThreadGroups, numDetailNoiseThreadGroups, numDetailNoiseThreadGroups);

            //noiseCompute.Dispatch (1, numDetailNoiseThreadGroups, numDetailNoiseThreadGroups, numDetailNoiseThreadGroups);

            if (logComputeTime) {
                // GetData call will halt execution until compute shader has completed execution, allowing the time to be measured
                int[] testData = new int[numBaseNoiseThreadGroups];
                testBuffer.GetData (testData);

                int totalTime = (int) (mainTimer.ElapsedMilliseconds + computeTimer.ElapsedMilliseconds);
                Debug.Log ($"Cloud noise: {totalTime}ms (C#: {mainTimer.ElapsedMilliseconds}ms Shader: {computeTimer.ElapsedMilliseconds}ms)");
            }

            // Release buffers
            foreach (var buffer in buffersToRelease) {
                buffer.Release ();
            }
        }
    }

    void UpdateSimplex () {
        var prng = new System.Random (simplexSettings.seed);
        CreateBuffer (new SimplexSettings[] { simplexSettings }, SimplexSettings.Size, "simplexSettingsBuffer");
        CreateRandomOffsetsBuffer (simplexSettings.numLayers, prng, "simplexOffsets");
        noiseCompute.SetInt ("resolution", baseResolution);
    }

    void UpdateWorley (WorleySettings settings) {
        var prng = new System.Random (settings.seed);

        int totalPointCount = 0;
        float frequency = 1;
        for (int i = 0; i < settings.numLayers; i++) {
            totalPointCount += Mathf.FloorToInt (settings.numPoints * frequency);
            frequency *= settings.lacunarity;
        }

        Vector3[] points = new Vector3[totalPointCount];

        for (int i = 0; i < points.Length; i++) {
            points[i] = new Vector3 ((float) prng.NextDouble (), (float) prng.NextDouble (), (float) prng.NextDouble ());
        }

        CreateBuffer (points, sizeof (float) * 3, "points");
        CreateBuffer (new WorleySettings[] { settings }, WorleySettings.Size, "worleySettingsBuffer");
    }

    // Create buffer of random 3d vectors
    void CreateRandomOffsetsBuffer (int numOffsets, System.Random prng, string bufferName, int kernel = 0) {
        float scale = 1000;
        Vector3[] offsets = new Vector3[numOffsets];
        for (int i = 0; i < offsets.Length; i++) {
            offsets[i] = new Vector3 ((float) prng.NextDouble () * 2 - 1, (float) prng.NextDouble () * 2 - 1, (float) prng.NextDouble () * 2 - 1) * scale;
        }
        CreateBuffer (offsets, sizeof (float) * 3, bufferName, kernel);
    }

    // Create buffer with some data, and set in shader. Also add to list of buffers to be released
    ComputeBuffer CreateBuffer (System.Array data, int stride, string bufferName, int kernel = 0) {
        var buffer = new ComputeBuffer (data.Length, stride, ComputeBufferType.Raw);
        buffersToRelease.Add (buffer);
        buffer.SetData (data);
        noiseCompute.SetBuffer (kernel, bufferName, buffer);
        return buffer;
    }

    void CreateTexture (ref RenderTexture texture, int resolution) {
        if (texture == null || !texture.IsCreated () || texture.width != resolution || texture.height != resolution || texture.volumeDepth != resolution) {
            if (texture != null) {
                texture.Release ();
            }
            texture = new RenderTexture (resolution, resolution, 0, RenderTextureFormat.ARGB32);
            texture.volumeDepth = resolution;
            texture.enableRandomWrite = true;
            texture.dimension = UnityEngine.Rendering.TextureDimension.Tex3D;
            texture.wrapMode = TextureWrapMode.Repeat;

            texture.Create ();
            updateNoise = true;
        }
    }

    public void ManualUpdate () {
        updateNoise = true;
        UpdateNoise ();
    }

    void OnValidate () {
        updateNoise = true;
    }
}