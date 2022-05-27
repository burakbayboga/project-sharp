using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyType : MonoBehaviour
{
	public Enemy enemy;
	public abstract void MoveTurn();
	public abstract SkillType GetActionType();
	public abstract void Init(Enemy _enemy);
}
