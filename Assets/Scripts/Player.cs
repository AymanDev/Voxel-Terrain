using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {
    [SerializeField] private Rigidbody rigidbody;
    [SerializeField] private float speed = 10f;

    private void Awake() {
        var spawnPos = transform.localPosition;
        spawnPos.y = 256;
        transform.localPosition = spawnPos;
    }

    private void FixedUpdate() {
        var vertical = Input.GetAxis("Vertical");
        var horizontal = Input.GetAxis("Horizontal");
        var velocity = rigidbody.velocity;


        if (Mathf.Abs(vertical) > 0) {
            velocity = transform.forward * (vertical * speed * Time.deltaTime);
        }

        if (Mathf.Abs(horizontal) > 0) {
            velocity = transform.right * (horizontal * speed * Time.deltaTime);
        }

        rigidbody.velocity = velocity;

        var mouseX = Input.GetAxis("Mouse X");
        var mouseY = Input.GetAxis("Mouse Y");

        if (Mathf.Abs(mouseX) > 0) {
            transform.Rotate(Vector3.up, mouseX);
        }

        if (Mathf.Abs(mouseY) > 0) {
            // transform.Rotate(Vector3.forward, -mouseY);
        }
    }
}