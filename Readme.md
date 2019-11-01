This is an interactive grass shader. It fakes some wind (strength and speed is adjustable) and also bends around multiple colliders.
When setting a collider to the layer "GrassCutLayer" you can "cut" the grass too.
Cutting grass works by shrinking it to 0.1f height and emitting grass blade particles at the cut position. 
Grass regrows over time. 
Collision and growing is calculated using a script but the grass is drawn using GPU instancing command using an optimized array with all the information like size, bending etc. 
The Wind bending is additionally to the other bending and is done using noise function in the vertex shader.
It's currently a normal HLSL shader and not yet a ShaderGraph. Shadow casting works in the demo but is sometimes offset by some value in my other scenes. 
Maybe due to the scaling of some object. ShaderGraph isn't possible yet because you cannot set custom properties per object in SG yet.

Scene:
[Scene](https://github.com/Fonteinsoft/GrassShader/blob/master/scene.PNG)


![Scene](https://github.com/Fonteinsoft/GrassShader/blob/master/scene.PNG)

[![Watch the video](https://img.youtube.com/vi/T-D1KVIuvjA/maxresdefault.jpg)](https://youtu.be/T-D1KVIuvjA)

[![Demo Doccou alpha](http://share.gifyoutube.com/KzB6Gb.gif)](https://www.youtube.com/watch?v=ek1j272iAmc)

Gif:
<div align=center>
<img src="https://thumbs.gfycat.com/SpiffyMagnificentAmazondolphin-mobile.mp4"/>
</div>