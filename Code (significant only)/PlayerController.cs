using System;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;


public class PlayerController : MonoBehaviour
{ 
private enum State
{ 
idle,
running,
jumping,
falling,
hurt,
climb,
crouching
}

private Rigidbody2D rb;

private Animator anim;

private Collider2D coll;

private PlayerController.State state;

[SerializeField]
private LayerMask ground;

[SerializeField]
private float speed;

[SerializeField]
private float jumpForce = 8f;

[SerializeField]
private float hurtForce;

[SerializeField]
private float SpikeSpeed;

[SerializeField]
private AudioSource cherry;

[SerializeField]
private AudioSource footstep;

[HideInInspector]
public bool canClimb;

[HideInInspector]
public bool bottomLadder;

[HideInInspector]
public bool topLadder;

public Ladder ladder;

private float naturalGravity;

[SerializeField]
private float climbSpeed = 3f;

private bool canDoubleJump;

private bool jumpbreaker;

public bool OnMovingPlatform;

private void Start()
{ 
this.rb = base.GetComponent<Rigidbody2D>();
this.anim = base.GetComponent<Animator>();
this.coll = base.GetComponent<Collider2D>();
if (PermanentUI.perm.isHealth100)
{ 
PermanentUI.perm.health = 20;
}
PermanentUI.perm.healthAmount.text = PermanentUI.perm.health.ToString();
this.naturalGravity = this.rb.gravityScale;
}

private void Update()
{ 
if (this.state == PlayerController.State.climb)
{ 
this.Climb();
}
if (this.state != PlayerController.State.hurt && this.state != PlayerController.State.crouching)
{ 
float axis = Input.GetAxis("Horizontal");
if (this.canClimb && Mathf.Abs(Input.GetAxis("Vertical")) > 0.05f)
{ 
this.state = PlayerController.State.climb;
this.rb.gravityScale = 0f;
this.rb.constraints = (RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation);
base.transform.position = new Vector3(this.ladder.transform.position.x, this.rb.position.y);
}
if (Mathf.Round(this.rb.gravityScale) > 0f)
{ 
if (axis < 0f)
{ 
this.rb.velocity = new Vector2(-this.speed, this.rb.velocity.y);
base.transform.localScale = new Vector2(-1f, 1f);
}
else if (axis > 0f)
{ 
this.rb.velocity = new Vector2(this.speed, this.rb.velocity.y);
base.transform.localScale = new Vector2(1f, 1f);
}
}
if (Mathf.Round(this.rb.gravityScale) < 0f)
{ 
if (axis < 0f)
{ 
this.rb.velocity = new Vector2(-this.speed, this.rb.velocity.y);
base.transform.localScale = new Vector2(1f, 1f);
}
else if (axis > 0f)
{ 
this.rb.velocity = new Vector2(this.speed, this.rb.velocity.y);
base.transform.localScale = new Vector2(-1f, 1f);
}
}
if (Input.GetButtonDown("Jump"))
{ 
if (this.coll.IsTouchingLayers(this.ground))
{ 
this.Jump();
this.canDoubleJump = true;
}
else if (this.canDoubleJump)
{ 
this.Jump();
this.canDoubleJump = false;
}
}
if (Input.GetButtonDown("Crouch") && this.coll.IsTouchingLayers(this.ground))
{ 
this.state = PlayerController.State.crouching;
}
}
this.AnimationState();
this.anim.SetInteger("state", (int)this.state);
axis
}

private void Jump()
{ 
if (!this.jumpbreaker)
{ 
if (Mathf.Round(this.rb.gravityScale) > 0f)
{ 
this.jumpForce = Mathf.Abs(this.jumpForce);
this.rb.velocity = new Vector2(this.rb.velocity.x, this.jumpForce);
this.state = PlayerController.State.jumping;
}
if (Mathf.Round(this.rb.gravityScale) < 0f)
{ 
this.jumpForce = -Mathf.Abs(this.jumpForce);
this.rb.velocity = new Vector2(this.rb.velocity.x, this.jumpForce);
this.state = PlayerController.State.jumping;
}
}
}

private void OnTriggerEnter2D(Collider2D collision)
{ 
if (collision.tag == "Collectable")
{ 
this.cherry.Play();
UnityEngine.Object.Destroy(collision.gameObject);
PermanentUI.perm.cherries++;
PermanentUI.perm.cherryText.text = PermanentUI.perm.cherries.ToString();
}
if (collision.tag == "Powerup")
{ 
UnityEngine.Object.Destroy(collision.gameObject);
this.jumpForce = 10f;
base.GetComponent<SpriteRenderer>().color = Color.red;
base.StartCoroutine(this.ResetPower());
}
if (collision.tag == "Spike")
{ 
this.jumpForce = 5f;
this.speed = 3f;
this.rb.velocity = new Vector2(this.speed, this.rb.velocity.y);
base.GetComponent<SpriteRenderer>().color = Color.black;
base.StartCoroutine(this.LossPower());
}
if (collision.tag == "JumpBreaker")
{ 
this.jumpbreaker = true;
this.state = PlayerController.State.idle;
}
}

private void OnTriggerExit2D(Collider2D collision)
{ 
if (collision.tag == "JumpBreaker")
{ 
this.jumpbreaker = false;
}
}

private void OnCollisionExit2D(Collision2D collision)
{ 
if (collision.gameObject.tag == "MovingPlatform")
{ 
base.transform.parent = null;
}
}

private void OnCollisionEnter2D(Collision2D other)
{ 
if (other.gameObject.tag == "MovingPlatform")
{ 
base.transform.parent = other.gameObject.transform;
}
if (other.gameObject.tag == "Enemy")
{ 
Enemy component = other.gameObject.GetComponent<Enemy>();
if (this.state == PlayerController.State.falling)
{ 
component.JumpedOn();
this.Jump();
return;
}
this.state = PlayerController.State.hurt;
if (Mathf.Round(this.rb.gravityScale) > 0f)
{ 
base.transform.localScale = new Vector2(1f, 1f);
}
if (Mathf.Round(this.rb.gravityScale) < 0f)
{ 
base.transform.localScale = new Vector2(1f, -1f);
}
this.HandleHealth();
if (other.gameObject.transform.position.x > base.transform.position.x)
{ 
this.rb.velocity = new Vector2(-this.hurtForce, this.rb.velocity.y);
return;
}
this.rb.velocity = new Vector2(this.hurtForce, this.rb.velocity.y);
}
component
}

private void HandleHealth()
{ 
PermanentUI.perm.isHealth100 = false;
PermanentUI.perm.health--;
PermanentUI.perm.healthAmount.text = PermanentUI.perm.health.ToString();
if (PermanentUI.perm.health == 0)
{ 
PermanentUI.perm.health = 20;
SceneManager.LoadScene(SceneManager.GetActiveScene().name);
}
}

private void AnimationState()
{ 
if (this.state == PlayerController.State.crouching)
{ 
if (Input.GetButtonUp("Crouch"))
{ 
Debug.Log("C key was released.");
this.state = PlayerController.State.idle;
return;
}
}
else if (this.state == PlayerController.State.jumping)
{ 
if (Mathf.Round(this.rb.gravityScale) > 0f && this.rb.velocity.y < 0.1f)
{ 
this.state = PlayerController.State.falling;
}
if (Mathf.Round(this.rb.gravityScale) < 0f && this.rb.velocity.y > -0.1f)
{ 
this.state = PlayerController.State.falling;
return;
}
}
else if (this.state == PlayerController.State.falling)
{ 
if (this.coll.IsTouchingLayers(this.ground))
{ 
this.state = PlayerController.State.idle;
return;
}
}
else if (this.state == PlayerController.State.hurt)
{ 
if (Mathf.Abs(this.rb.velocity.x) < 0.1f)
{ 
this.state = PlayerController.State.idle;
return;
}
}
else
{ 
if (Mathf.Abs(this.rb.velocity.x) > 1f)
{ 
this.state = PlayerController.State.running;
return;
}
this.state = PlayerController.State.idle;
}
}

private void Footstep()
{ 
this.footstep.Play();
}

[IteratorStateMachine(typeof(PlayerController.<ResetPower>d__31))]
private IEnumerator ResetPower()
{ 
int num;
while (num == 0)
{ 
yield return new WaitForSeconds(10f);
}
if (num != 1)
{ 
yield break;
}
this.jumpForce = 8f;
base.GetComponent<SpriteRenderer>().color = Color.white;
yield break;
num
}

[IteratorStateMachine(typeof(PlayerController.<LossPower>d__32))]
private IEnumerator LossPower()
{ 
int num;
while (num == 0)
{ 
yield return new WaitForSeconds(20f);
}
if (num != 1)
{ 
yield break;
}
this.jumpForce = 8f;
this.speed = 5f;
float expr_5E = Input.GetAxis("Horizontal");
if (expr_5E > 0f)
{ 
this.rb.velocity = new Vector2(this.speed, this.rb.velocity.y);
}
if (expr_5E < 0f)
{ 
this.rb.velocity = new Vector2(-this.speed, this.rb.velocity.y);
}
base.GetComponent<SpriteRenderer>().color = Color.white;
yield break;
num
expr_5E
}

private void Climb()
{ 
if (Input.GetButtonDown("Jump"))
{ 
this.rb.constraints = RigidbodyConstraints2D.FreezeRotation;
this.canClimb = false;
this.rb.gravityScale = this.naturalGravity;
this.anim.speed = 1f;
this.Jump();
return;
}
float axis = Input.GetAxis("Vertical");
if (axis > 0.1f && !this.topLadder)
{ 
this.rb.velocity = new Vector2(0f, axis * this.climbSpeed);
this.anim.speed = 1f;
return;
}
if (axis < -0.1f && !this.bottomLadder)
{ 
this.rb.velocity = new Vector2(0f, axis * this.climbSpeed);
this.anim.speed = 1f;
return;
}
this.anim.speed = 0f;
this.rb.velocity = Vector2.zero;
axis
}

public void Transform()
{ 
Vector3 localScale = base.transform.localScale;
localScale.x *= 1f;
base.transform.localScale = localScale;
localScale
}
}
