using UnityEngine;
using System;

/*

will stop the the user entering a value below (or equal to) this bound

*/
public class ClampMinAttribute : PropertyAttribute {
	
	// record what type was set, so we can cry if someone mismatches a bound and a property
	public int IntBound { 
		get {
			if (FixedType != typeof(int)) {
				throw new UnityException(FixedType.ToString() + " is set, asked for int in " + GetType());
			}
			return m_intBound;
		} protected set {
			FixedType = typeof(int);
			m_intBound = value;
		}
	}
	
	public float FloatBound { 
		get {
			if (FixedType != typeof(float)) {
				throw new UnityException(FixedType.ToString() + " is set, asked for float in " + GetType());
			}
			return m_floatBound;
		}
		protected set {
			FixedType = typeof(float);
			m_floatBound = value;
		}
	}
	
	//////////////////////////////////////////////////
	
	private Type FixedType { get; set; }
	private float m_floatBound;
	private int m_intBound;
	
	//////////////////////////////////////////////////
	
	public ClampMinAttribute(int lowerBound) { 
		IntBound = lowerBound;
	}
	public ClampMinAttribute(float lowerBound) {
		FloatBound = lowerBound;
	}
}