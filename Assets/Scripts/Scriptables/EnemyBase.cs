using UnityEngine;

[CreateAssetMenu(fileName = "Enemy", menuName = "Enemy/Create New Enemy")]
public class EnemyBase : ScriptableObject
{
    [SerializeField] public string _name;
    [SerializeField] public int _caseNo; 
    [SerializeField] public float _speed;
    [SerializeField] public float _attackPower;
    [SerializeField] public float _attackSpeed;
    [SerializeField] public float _attackRange;
    [SerializeField] public float _maxHealth;
    [SerializeField] public float _regenRate;
    [SerializeField] public float _defense;
    [SerializeField] public float _aggroRange;
    [SerializeField] public float _maxAggroRange;//max agro range den çýkýlýnca karakter takip etmeyi býrakýr ve isaggro false olur. aggro range den biraz fazla olucak
    [SerializeField] public bool _isInfected;

}
