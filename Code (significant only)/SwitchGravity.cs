using System;
using UnityEngine;


public class SwitchGravity : MonoBehaviour
{ 
private Rigidbody2D rb;

private PlayerController Player;

private bool top;

private void Start()
{ 
this.Player = base.GetComponent<PlayerController>();
this.rb = base.GetComponent<Rigidbody2D>();
}

private void Update()
{ 
if (Input.GetKeyDown(KeyCode.G))
{ 
this.rb.gravityScale *= -1f;
this.Rotation();
this.Player.Transform();
}
}

private void Rotation()
{ 
if (!this.top)
{ 
base.transform.eulerAngles = new Vector3(0f, 0f, 180f);
}
else
{ 
base.transform.eulerAngles = new Vector3(0f, 0f, 0f);
}
this.top = !this.top;
}

private void Face()
{ 
float axis = Input.GetAxis("Horizontal");
if (Mathf.Round(this.rb.gravityScale) < 0f)
{ 
if (axis > 0f)
{ 
base.transform.localScale = new Vector2(-1f, 1f);
}
if (axis < 0f)
{ 
base.transform.localScale = new Vector2(1f, 1f);
}
}
axis
}
}