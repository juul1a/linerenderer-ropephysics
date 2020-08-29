# linerenderer-ropephysics

Unity C# script (Incomplete)
I do not claim ownership to this code!! I have only made a few additions.
The bulk of this code is from Jason Yang aka Lang Ho Doo in this youtube tutorial: https://www.youtube.com/watch?v=FcnvwtyxLds
Here is his original source code: https://github.com/dci05049/Verlet-Rope-Unity

Instructions:
2D Project
Attach this script to an empty game object
Add a line renderer component
Add an edge collider component
Put a sprite in the scene with rigidbody and a collider (freeze X & Y position & Z Rotation)


What I am trying to do is add collisions to the rope so it can bump up against & wrap around things
So far: I've added an edge collider and set all of the edge collider points to match the rope points.
Then I check if the collider comes into contact with any other colliders and grab all contact points.
Then based on each contact point I check the rope points to see if they are nearby a contact point. If so I find their orientation relative to the contact point.
Then I basically ensure the x/y values of the rope point do not pass through the contact points.

This is probably not the best way to do it but I am still figuring this out. 
Open to any/all collaboration on this project so we can get some nice unity rope physics working for all!
