using UnityEngine;

public class InputController : MonoBehaviour
{
    [SerializeField] private float _force = 10f;
    
    
    private bool _frontLeft = false;
    private bool _frontRight = false;
    private bool _backLeft = false;
    private bool _backRight = false;
    
    
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        _frontLeft = false;
        _frontRight = false;
        _backLeft = false;
        _backRight = false;
        
        
        //// Front left
        //if (Input.GetKey(KeyCode.I))
        //{
        //    _frontLeft = true;
        //}
        //// Front right
        //if (Input.GetKey(KeyCode.P))
        //{
        //    _frontRight = true;
        //}
        //// Back left
        //if (Input.GetKey(KeyCode.Z))
        //{
        //    _backLeft = true;
        //}
        //// Back right
        //if (Input.GetKey(KeyCode.C))
        //{
        //    _backRight = true;
        //}
        
        Vector2 direction2D = Vector2.zero;
        // Front left
        if (Input.GetKey(KeyCode.W))
        {
            direction2D += Vector2.up;
        }
        // Front right
        if (Input.GetKey(KeyCode.S))
        {
            direction2D -= Vector2.up;
        }
        // Back left
        if (Input.GetKey(KeyCode.A))
        {
            direction2D += Vector2.left;
        }
        // Back right
        if (Input.GetKey(KeyCode.D))
        {
            direction2D += Vector2.right;
        }
        
        Vector3 direction = new Vector3(direction2D.x, 0f, direction2D.y);
        
        GetComponent<Rigidbody>().AddForce(direction * _force);
        
        
        
        GuiDebug.Instance.PrintString("FL", _frontLeft.ToString());
        GuiDebug.Instance.PrintString("FR", _frontRight.ToString());
        GuiDebug.Instance.PrintString("BL", _backLeft.ToString());
        GuiDebug.Instance.PrintString("BR", _backRight.ToString());
        
    }
}
