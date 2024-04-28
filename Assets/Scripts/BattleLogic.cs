using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using UnityEngine;
using System.Linq;

namespace BattleLogic
{
    [Serializable]
    public class CharStats
    {
        // Health
        public float Maxhp = 100;
        // Attack
        public float atk = 15;
        // Defence
        public float def = 15;
        // Efficiency (Percent)
        public float eff = 1;
        // Speed
        public float spd = 10;

        public CharStats(float Maxhp, float atk, float def, float eff, float spd)
        {
            this.Maxhp = Maxhp;
            this.atk = atk;
            this.def = def;
            this.eff = eff;
            this.spd = spd;
        }
        public override string ToString()
        {
            return string.Format("Maxhp: {0}, atk: {1}, def: {2}, eff: {3}, spd: {4}", Maxhp, atk, def, eff, spd);
        }
    }
    public enum Stance
    {
        None, Physical, Mental, Electric, Chemical, Chaotic, Natural, Super
    }
    public enum TargetType
    {
       None, Self, SingleEnemy, SingleAlly, SplashEnemy, SplashAlly, AoeEnemy, AoeAlly, AoeAll
    }
    [Serializable]
    public class Actor
    {
        public string name;
        public CharStats stats;
        public int id;
        public float hp;
        public Stance stance;
        // M means modified
        public float Mmhp;
        // Modified Max Hit Points
        public float Matk;
        public float Mdef;
        public float Meff;
        public float Mspd;
        public Actor(string name, CharStats stats, int id, float hp, Stance stance, float mmhp = 1, float matk = 1, float mdef = 1, float meff = 1, float mspd = 1)
        {
            this.name = name;
            this.stats = stats;
            this.id = id;
            this.hp = hp;
            this.stance = stance;
            Matk = matk;
            Mdef = mdef;
            Meff = meff;
            Mspd = mspd;
            Mmhp = mmhp;
        }

        public Actor TakeDmg(float damage)
        {
            float newHP = hp - damage;
            if (newHP <= 0) { Debug.Log(name + " is Dead!"); }
            return new Actor(name, stats, id, newHP, stance, Matk, Mdef, Meff, Mspd);  
        }
    }
    [Serializable]
    public class GameState
    {
        public ImmutableList<Actor> allies { get { return _allies; } set { _allies = value; visibleAllies = value.ToList(); } }
        public ImmutableList<Actor> enemies {  get { return _enemies; } set { _enemies = value; visibleEnemies = value.ToList(); } }
        private ImmutableList<Actor> _allies;
        private ImmutableList<Actor> _enemies;
        [SerializeField]
        private List<Actor> visibleAllies;
        [SerializeField]
        private List<Actor> visibleEnemies;
        public Actor currentActor;
        public Stance currentStance;
        public ImmutableDictionary<Stance, float> allyStancePoints;
        public ImmutableDictionary<Stance, float> enemyStancePoints;
        public IEnumerable<Actor> actors
        { get {  return allies.Concat(enemies); } }
        
        //public ImmutabeList<Action> effects;

        public GameState(
            ImmutableList<Actor> allies, 
            ImmutableList<Actor> enemies, 
            Actor currentActor, 
            Stance currentStance, 
            ImmutableDictionary<Stance, float> allyStancePoints, 
            ImmutableDictionary<Stance, float> enemyStancePoints) 
        {
            this.allies = allies;
            this.enemies = enemies;
            this.currentActor = currentActor;
            this.currentStance = currentStance;
            this.allyStancePoints = allyStancePoints;
            this.enemyStancePoints = enemyStancePoints;
        }

        public GameState WithActor(Actor actor) 
        {
            // Find Actor by ID (with linq)
            var oldactor = allies.FirstOrDefault(a => a.id == actor.id);
            // Replace Found Actor with modified one
            if (oldactor != null)
            {
                //Debug.Log(oldactor.Equals(actor));
                return WithAllies(allies.Replace(oldactor, actor));
            }
            oldactor = enemies.FirstOrDefault(a => a.id == actor.id);
            if (oldactor != null)
            {
                return WithEnemies(enemies.Replace(oldactor, actor));
            }

            throw new NotImplementedException();
        }
        public GameState SetCurrentActor(int id)
        {
            var newCurrentActor = allies.FirstOrDefault(a => a.id == id) ?? enemies.FirstOrDefault(a => a.id == id);
            if (newCurrentActor == null) 
            {
                Debug.LogError("Error in SetCurrentActor: Invalid ID :(");
                throw new ArgumentOutOfRangeException();
            }
            return new GameState(allies, enemies, newCurrentActor, currentStance, allyStancePoints, enemyStancePoints);
        }
        public Actor GetActor(int id) 
        {
            var foundActor = allies.FirstOrDefault(a => a.id == id) ?? enemies.FirstOrDefault(a => a.id == id);
            if (foundActor == null)
            {
                Debug.LogError("Error in GetStats: Invalid ID :(");
                throw new ArgumentOutOfRangeException();
            }
            return foundActor;
        }
        public CharStats GetStats(int id)
        {
            return GetActor(id).stats;
        }
        public GameState WithAllies(ImmutableList<Actor> Mallies) 
        {
            var newCurrentActor = Mallies.FirstOrDefault(a => a.id == currentActor.id) ?? currentActor;
            //newCurrentActor = newCurrentActor == null ? currentActor : newCurrentActor;
            return new GameState(Mallies, enemies, newCurrentActor, currentStance, allyStancePoints, enemyStancePoints);
        }
        public GameState WithEnemies(ImmutableList<Actor> Menemies)
        {
            var newCurrentActor = Menemies.FirstOrDefault(a => a.id == currentActor.id) ?? currentActor;
            return new GameState(allies, Menemies, newCurrentActor, currentStance, allyStancePoints, enemyStancePoints);
        }
        public bool IsIDAlly(int  id)
        {
           return allies.Any(a => a.id == id);
        }
        public bool IsIDEnemy(int id)
        {
            return enemies.Any(a => a.id == id);
        }
        public TypeCombo FindCharWeakness(int id) 
        {
            Stance charStance = GetActor(id).stance;
            Stance stateStance = currentStance;
            List<Stance> StanceWeaks = new List<Stance>();
            StanceWeaks.Add(Helper.FindWeakness(charStance));
            StanceWeaks.Add(Helper.FindWeakness(stateStance));
            List<Stance> StanceRes = new List<Stance>();
            StanceRes.Add(Helper.FindResistance(charStance));
            StanceRes.Add(Helper.FindResistance(stateStance));
            foreach (Stance stance in StanceWeaks)
            {
                if (StanceRes.Contains(stance))
                {
                    StanceWeaks.Remove(stance);
                    StanceRes.Remove(stance);
                }
            }
            return new TypeCombo(StanceWeaks, StanceRes);
        }
        
       /* public bool? IsIDAlly2(int id)
        {
            if (IsIDAlly(id)) { return true; }
            else if (IsIDEnemy(id)) { return false; }
            else { return null; }
        } */
    }
    public class TypeCombo 
    {
        public TypeCombo (List<Stance> weaknesses, List<Stance> resistances)
        {
            this.weaknesses = weaknesses;
            this.resistances = resistances;
        }

        public List<Stance> weaknesses;
        public List<Stance> resistances;
    }
    static class Helper
    {
        public static Stance FindWeakness(Stance stance)
        {
            switch (stance)
            {
                case Stance.Electric: return Stance.Natural;
                case Stance.Natural: return Stance.Chemical;
                case Stance.Chemical: return Stance.Electric;
                case Stance.Physical: return Stance.Mental;
                case Stance.Mental: return Stance.Chaotic;
                case Stance.Chaotic: return Stance.Physical;
                default : return Stance.None;
            }
        }
        public static Stance FindResistance(Stance stance)
        {
            switch (stance)
            {
                case Stance.Electric: return Stance.Chemical;
                case Stance.Natural: return Stance.Electric;
                case Stance.Chemical: return Stance.Natural;
                case Stance.Physical: return Stance.Chaotic;
                case Stance.Mental: return Stance.Physical;
                case Stance.Chaotic: return Stance.Mental;
                default: return Stance.None;
            }
        }
    }
}
