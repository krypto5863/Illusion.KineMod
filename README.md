# KineMod
A simple plugin meant mostly for us IK users who have noticed there's a painful lack of control over certain body parts that FK has.

- IK can now work with FK.
- Toggle individual FK bones such as clavicle, toes, etc,.
- Easy to use control over the IK effectors.

[CharaStudio_0IDcnxzZGj.webm](https://github.com/user-attachments/assets/329785b3-5195-46d2-9d61-53199e009921)

# Why
Originally this all started as a mere attempt at making the clavicle FK bones usable with IK. In trying to do so, I discovered the effectors, the way FK causes IK to be ignored, and other FK bones we IK users are missing out on, particularly toes. I decided I would give this control back to the user.

# Installation
Grab the corresponding DLL for your game in the releases and place it into Bepinex/plugins.

Not really compatible with FKIK, simply because it replaces it. But they can co-exist.
AdvIK's shoulder functions should not be used if you plan to use FK clavicle controls.

# IK & FK?
FK does not play nice with IK, using FK will render IK useless. This is because FK makes it's pose changes after IK has solved the pose. The result is IK is basically overwritten. To solve this, I patched the FK controller to force it to change the pose before IK has read the pose, allowing both to work perfectly, though FK does now have deference to IK.

However, if you made scenes or poses that have IK on but really poses with FK, you may find they're mangled. Simply disable IK and they'll be back to normal.

# Effectors?
Effectors control the effect an IK target has over the pose. When you set all effectors to 1, you basically pin the joint to the IK target. This is unfortunate because FinalIK is very advanced and can sometimes pose better than you can if you let it, since it has it's own defined joint angle limits. A pose that might take you several minutes, could take you less if you loosen your effectors and just move the one you want, allowing the body to follow the joint's movement. Below is an example of the pulling effect you can achieve when you just let the hand effector influence the pose.

[CharaStudio_wRR8S7fKe4.webm](https://github.com/user-attachments/assets/25cce515-e53c-460e-97b0-66f06590ad1a)
