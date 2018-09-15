using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRCS : MonoBehaviour {
	public List<ParticleSystem> pitchUpRCS = new List<ParticleSystem>();
	public List<ParticleSystem> pitchDownRCS = new List<ParticleSystem>();
	public List<ParticleSystem> yawLeftRCS = new List<ParticleSystem>();
	public List<ParticleSystem> yawRightRCS = new List<ParticleSystem>();
	public List<ParticleSystem> rollLeftRCS = new List<ParticleSystem>();
	public List<ParticleSystem> rollRightRCS = new List<ParticleSystem>();

	// Use this for initialization
	void Start () {
		foreach (ParticleSystem ps in GetComponentsInChildren<ParticleSystem>()) {
			if (ps.name.StartsWith("PitchUp"))
				this.pitchUpRCS.Add(ps);
			else if (ps.name.StartsWith("PitchDown"))
				this.pitchDownRCS.Add(ps);
			else if (ps.name.StartsWith("YawLeft"))
				this.yawLeftRCS.Add(ps);
			else if (ps.name.StartsWith("YawRight"))
				this.yawRightRCS.Add(ps);
			else if (ps.name.StartsWith("rollLeftRCS"))
				this.yawRightRCS.Add(ps);
			else if (ps.name.StartsWith("rollRightRCS"))
				this.yawRightRCS.Add(ps);
		}
	}

	public bool CalcPos(float axis){
		if (axis > 0)
			return true;
		else
			return false;
	}

	public void PlayParticle(int axisIndex, bool pos, float axis){
		List<ParticleSystem> rcsList = this.getRCS(axisIndex, pos);
		foreach (ParticleSystem ps in rcsList){
			var main = ps.main;
			main.startLifetime = 0.1f + Mathf.Abs(axis * 0.1f);
			ps.Play();
		}
	}

	public void StopParticle(int axisIndex, bool pos){
		List<ParticleSystem> rcsList = this.getRCS(axisIndex, pos);
		foreach (ParticleSystem ps in rcsList){
			ps.Stop();
		}
	}
	private List<ParticleSystem> getRCS(int axisIndex, bool pos){
		switch (axisIndex){
			case 0:
				if (pos)
					return this.pitchUpRCS;
				else
					return this.pitchDownRCS;
			case 1:
				if (pos)
					return this.yawLeftRCS;
				else
					return this.yawRightRCS;
			case 2:
				if (pos)
					return this.rollLeftRCS;
				else
					return this.rollRightRCS;
			default:
				return null;
		}
	}
}
