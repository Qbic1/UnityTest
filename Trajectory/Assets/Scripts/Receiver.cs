using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Receiver : MonoBehaviour {
	public float X { get; set; }
	public float Y { get; set; }
	private const int SPEED = 1000;

	public List<float> Time { get; set; }
	public List<float> Dist { get; set; }

	public void CalculateDist() {
		if(this.Time != null) {
			this.Dist = new List<float> ();
			foreach (float time in this.Time) {
				this.Dist.Add(time * SPEED * 1000);
			}
		}
	}
}
