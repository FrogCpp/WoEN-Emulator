using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TabScript : MonoBehaviour
{
    public float Target = 0;

    [SerializeField] private float k;

    private RectTransform me;
    private float Base;

    private void Start()
    {
        me = GetComponent<RectTransform>();
        Base = me.position.x;
    }

    private void Update()
    {
        if (me.position.x != (Base + Target))
        {
            var a = me.position;
            a.x += ((Base + Target) - a.x) * k;
            if (Mathf.Abs((Base + Target) - a.x) < 0.1)
            {
                a.x = (Base + Target);
            }
            me.position = a;
        }
    }
}
