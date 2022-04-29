using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IngameInput : MonoBehaviour
{

    public static IngameInput instance;

    public bool IsIngameInputActive;

    Camera MainCamera;
    int CreatureLayerMask;

    void Awake()
    {
        instance = this;
        CreatureLayerMask = 1 << 8;
        IsIngameInputActive = true;
    }

    void Start()
    {
        MainCamera = Camera.main;
    }

    void Update()
    {
        if (IsIngameInputActive && !GameController.instance.IsGameOver)
        {
            if (Input.GetMouseButtonDown(0))
            {
                RaycastHit hit;
                Ray ray = MainCamera.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit, 100f, CreatureLayerMask))
                {
                    if (hit.collider.CompareTag("Player"))
                    {
                        GameController.instance.OnPlayerClicked();
                    }
                    else if (hit.collider.CompareTag("Enemy"))
                    {
                        Enemy enemy = hit.collider.GetComponent<Enemy>();
                        GameController.instance.OnEnemyClicked(enemy);
                    }
                }
                else
                {
                    GameController.instance.OnEmptyClick();
                }
            }
        }
    }

    void LateUpdate()
    {
        IsIngameInputActive = true;
    }


}
