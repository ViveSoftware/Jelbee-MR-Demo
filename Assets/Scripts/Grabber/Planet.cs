using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Planet : Grabbable
{
	[SerializeField]
	private float minRotSpeed = 1;
	[SerializeField]
	private float maxRotSpeed = 2;

    public MeshRenderer PlanetObject;

	private Vector3 planetRotation;
	// Private variables
	private Transform _cacheTransform;

	void Start()
	{
		// Cache reference to transform to improve performance
		_cacheTransform = transform;
		planetRotation = Random.insideUnitSphere.normalized * Random.Range(minRotSpeed, maxRotSpeed);
	}

	void Update()
	{
		// Rotate the planet based on the rotational vector
		if (_cacheTransform != null)
		{
			_cacheTransform.Rotate(planetRotation * Time.deltaTime);
		}
	}
}
