using UnityEngine;
using System.Collections;

public class ForceToAcessibleVolumeVR : MonoBehaviour
{
	public int accesibleVolumeLayer = 13;
	private Vector3 lastContanctPosition;

	private bool outsideVolume = false;
	private CharacterControllerVR player;

	void Start()
	{
		player = GetComponent<CharacterControllerVR>();
		lastContanctPosition = transform.position;
	}

	void OnTriggerStay(Collider other)
	{
		if (outsideVolume && other.gameObject.layer == accesibleVolumeLayer)
		{
			lastContanctPosition = transform.position;
			outsideVolume = false;
		}
	}

	void OnTriggerExit(Collider other)
	{
		if (other.gameObject.layer == accesibleVolumeLayer)
		{
			outsideVolume = true;
		}
	}

	void LateUpdate()
	{
		if (outsideVolume)
		{
			transform.position = lastContanctPosition;
		}
	}
}
