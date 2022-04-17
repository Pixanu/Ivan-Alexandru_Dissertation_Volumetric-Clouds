# Ivan Alexandru_Dissertation_Volumetric Clouds
 
# Procedural Generation of Volumetric Clouds in Video Games Based on Real-Time Rendering - Project P132827

This study presents a way of rendering volumetric clouds in Unity by using precalculated 3D cloud textures and real-time shader calculations. This project presents and evaluates a volumetric cloud implementation that aims to further improve aspects of the already published solutions such as:

- Andrew Schneider's implementation (2015) for Horizon Zero Dawn; 
- Frederik Haggstrom (2018) thesis;
- Dean Babić (2018) Master thesis;

Credits given to Sebastian Lague (2019) for his implementaion for Noise Generation and Weather Map. <br />
(https://www.youtube.com/watch?v=4QOcCGI6xOU&ab_channel=SebastianLague) <br />
License in the repository and in the used scripts.

## How to use
Navigate in the Unity Package Folder and get the  "Ivan Alexandru_Volumetric Clouds.rar"

Please follow the installation instructions below:

1. Extract the archive in your designated folder.

2. Open Unity/UnityHub and create a new project using Unity Version 2019.4.34f1 or newer.

3. Go to Assets Tab -> Import Package -> Custom Package -> navigate to where the content from the archive was extracted and select "Ivan Alexandru_Volumetric Clouds.unitypackage".

4. After importing, navigate towards the Assets Folder -> Scenes -> open the scene in that folder.

5. Press Play 

Attached to the Main Camera is the Clouds Master Script where all the customizations options are available, such as Ray-March Settings, Base Shape of the clouds, amount of Details of the clouds, Lighting and Animation options.

## Problems
Due to a problem in which the clouds prefab does not load correctly when added to a new scene, you can only see the volumetric clouds in the provided scene right now. If you want to use the prefab and adapt it to a new scene, you must set up all of its parameters.


