COMP 476 Assignment 2
-----------------------------

Author: Christian Plourde
I.D.: 26572499

This project is divided into many parts, all outlined below.

At the root of the folder we find two images. One is called DecisionTree.png and contains a visual representation of the decision tree used for dictating the behaviour of the non "IT" players in R3. The other is called StateMachine.png and this one shows a visual representation of the state machine used to dictate the behaviour of the "IT" player in R3.

Next we have a folder called BlenderModels. This folder just contains the models used for the characters and the decorative elements of the scene in R3.

Next we have a folder called FillScreenshots that contains the screenshots of the fills for R2. These screenshots are named appropriately.

Next we have two folders that contain the actual Unity Projects. 

The folder COMP-476-A2 contains the requirements for R2. This Unity Project has two scenes. One is called Graph, and this can be used to examine the fills of the pathfinding graph based on the different heuristics. In order to test a different heuristic, click on the GraphManager game object when the scene is loaded (using Unity) in the scene heirarchy and switch the heuristic type on its Graph manager script in the inspector using the drop down menu. It is important that this is done before pressing start. Attempting to change the heuristic type mid execution may cause errors. In order to trace a new path, simply click on a node and the path to it will be shown as well as the fill. The other scene is called NavMesh and uses Unity's navmesh for pathfinding. Simply click anywhere on the floor of the level and the character will move there.

The next folder (COMP-476-A2-TAG) contains the requirements for R3. Open the scene called Graph using Unity and run the program to see the game play out inifinitely. If the character who was "IT" is tagged, a random character from the players is seleected as the next "IT" player and the game resets.

For both Unity Projects, the C# scripts are found in the Assets Folder, in the Scripts folder.

The Scripts folder contains many files, explained below.

Character.cs : Contains all of the information about the players of the game for both R3 and R2. It has the decision tree and finite state machine used in R3 as well as all the update methods for pathfinding based on the different behaviours. It is responsible for doing  the pathfinding for all of the NPC's.

ClusterLookupTable.cs : Contains a lookup table for the Graph to find the distances between clusters of the graph when using the cluster heuristic is used for pathfinding. An instance of this is created when the graph is initialized, for subsequent lookups that are very fast. This was implemented using an nxn array.

Graph.cs : Contains two classes. One is a regular graph and calling its evaluate method will use Dijkstra's algorithm to compute the shortest path. The second is a pathfinder graph that used a* to compute the shortest path. These two classes are generic and so they can be used with any type of object. They also have a requirement that the type used must implement the IHeuristic interface defined in this file. This gurantees that the type used will have a function that allows it to compute a heuristic for use in the a* algorithm.

GraphManager.cs : Responsible for instantiating the graph based on all the nodes in the scene and linking the nodes properly.

LevelNode.cs : This script contains the script to be placed on the nodes in the scene. Each node contains a list of neighbors (also level nodes) that need to be set manually. This is the type used in our instance of the graph. The nodes also have a list of line renderers used to draw the paths on the graph.

Movement.cs : This file contains a class heirarchy for the different movement types for the AI. It is essentially the same as what I had in my first assignment, with the exception of the new steering arrive behaviour which is used for movement of npc's in r3.

NavMeshTest.cs : This file contains a simple update function that allows the user to click anywhere on the floor in the NavMesh scene of R2 to make the character move there using the navmesh.

NPC.cs : This is the base class for the character class defined in Character.cs. It contains fields and accessors for maximum velocity, acceleration, current position, etc. Basic properties of the state of an npc.