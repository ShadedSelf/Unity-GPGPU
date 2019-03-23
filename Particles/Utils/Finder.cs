using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Finder
{
	public static T FindObject<T>(string name) where T : UnityEngine.Object
	{
		T[] resources = (T[])Resources.FindObjectsOfTypeAll(typeof(T));
		
		foreach	(var res in resources)
			if (res.name == name) 
				return res;
		return null;
	}
}
