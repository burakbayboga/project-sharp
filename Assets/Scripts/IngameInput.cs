using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IngameInput : MonoBehaviour
{

    public static IngameInput instance;

    public bool IsIngameInputActive;

    Camera MainCamera;
    int creatureLayermask;
	int hexLayermask;
	int interactableLayermask;

    void Awake()
    {
        instance = this;
        creatureLayermask = 1 << 8;
		hexLayermask = 1 << 9;
		interactableLayermask = 1 << 11;
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
				int layermask = 1 << 20;
				if (GameController.instance.CurrentTurnState == TurnState.PlayerMovement)
				{
					layermask = hexLayermask;
				}
				else if (GameController.instance.CurrentTurnState == TurnState.PlayerAnswer)
				{
					if (GameController.instance.isSidestepActive)
					{
						layermask = hexLayermask;
					}
					else
					{
						layermask = creatureLayermask | interactableLayermask;
					}
				}
				RaycastHit2D[] hits = Physics2D.RaycastAll(MainCamera.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, 100f, layermask);
                if (hits.Length > 0)
                {
                    if (hits[0].collider.CompareTag("Player"))
                    {
                        GameController.instance.OnPlayerClicked();
                    }
                    else if (hits[0].collider.CompareTag("Enemy"))
                    {
                        Enemy enemy = hits[0].collider.GetComponent<Enemy>();
						GameController.instance.OnEnemyClicked(enemy);
                    }
					else if (hits[0].collider.CompareTag("hex"))
					{
						hits[0].collider.GetComponent<Hex>().HandleInput();
					}
					else if (hits[0].collider.CompareTag("interactable"))
					{
						Hex hex = hits[0].collider.GetComponent<Hex>();
						GameController.instance.OnInteractableHexClicked(hex);
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
