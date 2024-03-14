# Call-of-Duty-Movement-in-Godot-4
A Godot 4 project that imitates the movement mechanics of Call of Duty games

It features:
* Almost the same spatial movement due to being based on Quake 3's movement system just like Call of Duty's has been 
* Jumping (bunny hopping is possible)
* Standing, Crouching, Proning
* Sprinting, Tactical Sprinting from Modern Warfare 2019
* Camera Tilt during Sprinting or Tac Sprinting
* A stamina bar for tactical sprinting, made purely out of engine's built in assets
* Smooth Camera Follow (with lerping)
* Smooth Mouse Look (Smoothing is optional, you can disable the smoothing and get the raw look if you set the "Smoothing" value to 1)

I'm planning to add:
* Climbing mechanics
* Swimming mechanics
* Idle Sway
* Physical interactions with other physics objects

  I'll try to keep my sanity while trying to do all these as optimized as possible

In order to maximize the code optimization I have:

* Used engine calls as few as possible, and cached references when I've done as such
* Used Resources as basically the same way Unity Components work with the GameObjects in order to organize the code, the hierarchy and improve performance, Resources are a whole lot lighter than Nodes so I assume that their inherited scripts would bring similar results.
* Cached the Input Action names as public static readonly StringName variables in a public static class called "Mappings", just like how Godot itself caches the SignalName variables


**IMPORTANT**

* If the Resource Components (such as Movement and Look) are empty (null) in the editor, the script with instance a new one with default variables,
if not, the script will make your Resource Components unique.

* You can **still** edit or read your Resource Components and its values just like you'd do with Nodes in play mode, using the "Remote" tab in the SceneTree editor
  
![image](https://github.com/TheHyper-Dev/Call-of-Duty-Movement-in-Godot-4/assets/32967925/e8f0d6c9-e716-4698-847b-060e5361e978)

Here's a screenshot of the Movement Component upon clicking it

![image](https://github.com/TheHyper-Dev/Call-of-Duty-Movement-in-Godot-4/assets/32967925/c0dea0ea-51b2-448a-b7dd-a5aaa6544819)
 

NOT FINISHED YET, will rewrite entire CharacterBody3D logic in the far future because it fucking sucks

A proper ReadMe is coming soon with important things to note.

Game.cs must be set as an Autoload

The script no longer uses Input Actions set from the Godot Editor due to preventing needless engine calls, it uses a custom input sorting method
![image](https://github.com/TheHyper-Dev/Call-of-Duty-Movement-in-Godot-4/assets/32967925/edc76794-21a4-479c-961f-d5ff18f6f41a)

This is where all the player component inputs get processed, you may create sub input-sorting methods inside the components depending on the input device of course
![image](https://github.com/TheHyper-Dev/Call-of-Duty-Movement-in-Godot-4/assets/32967925/171adc2e-54e1-43d4-a90d-a75b57c43f07)


Read comments in the codes for further explanations.

My discord if you have any questions, I'll try to help as much as I can: **the_hyper_dev**
