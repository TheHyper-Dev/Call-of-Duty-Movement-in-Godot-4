# Call-of-Duty-Movement-in-Godot-4
A Godot 4 project that imitates the movement mechanics of Call of Duty games

It features:
* Almost the same spatial movement due to being based on Quake 3's movement system just like Call of Duty's has been 
* Jumping (duh)
* Standing, Crouching, Proning
* Sprinting, Tactical Sprinting from Modern Warfare 2019 (Gonna implement it tomorrow)

I'm planning to:
* Add climbing mechanics
* Add swimming mechanics
* Idle Sway
* Camera Tilt during Sprinting or Tac Sprinting
* Physical interactions with other physics objects 
* Keep my sanity while trying to do all these as optimized as possible

In order to maximize the code optimization I have:

* Used engine calls as few as possible, and cached references when I've done as such
* Used Resources as basically the same way Unity Components work with the GameObjects in order to organize the code, the hierarchy and improve performance, Resources are a whole lot lighter than Nodes so I assume that their inherited scripts would bring similar results.


**IMPORTANT**

* If the Resource Components (such as Movement and Look) are empty (null) in the editor, the script with instance a new one with default variables,
if not, the script will make your Resource Components unique.

* You can **still** edit or read your Resource Components and its values just like you'd do with Nodes in play mode, using the "Remote" tab in the SceneTree editor
  
![image](https://github.com/TheHyper-Dev/Call-of-Duty-Movement-in-Godot-4/assets/32967925/e8f0d6c9-e716-4698-847b-060e5361e978)

Here's a screenshot of the Movement Component upon clicking it

![image](https://github.com/TheHyper-Dev/Call-of-Duty-Movement-in-Godot-4/assets/32967925/c0dea0ea-51b2-448a-b7dd-a5aaa6544819)
 

NOT FINISHED YET

A proper ReadMe is coming soon with important things to note.

GameManager.cs must be set as an Autoload

You need to assign these Input Actions by clicking in Projects/Project Settings/Input Map
![image](https://github.com/TheHyper-Dev/Call-of-Duty-Movement-in-Godot-4/assets/32967925/b76609d5-bf26-4851-8fbb-4f76ff21dad9)


Read comments in the codes for further explanations.

My discord if you have any questions, I'll try to help as much as I can: **the_hyper_dev**
