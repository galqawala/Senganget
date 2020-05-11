Read the example scripts and you will know how "Advanced Save System" works.

If you have problems, these are the most common mistakes:
-) If you want to use the save system, don't forget the "using SaveSystem;" statement.
-) Saving an object:
	-) Your class must be public.
	-) The variables you want to save must be public.
	-) Your class must have a constuctor with no parameters.
	-) Your class must have the [System.Serializable] attribute.
	-) Your class must not derive from any other class.

For further questions, feel free to contact me here: hendrik.haidenthaler@gmail.com