using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using UnityEngine;
using System.Linq;
using UnityEditor.SceneManagement;
using UnityEngine.Assertions.Must;

namespace BattleLogic
{

    public enum BuffStat
    {
        Attack,
        Defense,
        Speed
    }

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

    /*
        New idea: Make a system that allows limited number of recruits per area.
        Allows every run to be destinct
     
        True completionists must get the true completion by winning the game with each charichter recrutied, will take multiple runs.

     */


    public class ActorStatus
    {
        public bool downed = false;
        public bool dead = false;
        // Effects may go here but idk
        public ActorStatus()
        {
            downed = false;
            dead = false;
        }
        public ActorStatus(bool downed, bool dead)
        {
            this.downed = downed;
            this.dead = dead;
        }

        public ActorStatus Down() 
        {
            return new ActorStatus(true, dead);
        }

        public ActorStatus Recover()
        {
            return new ActorStatus(false, dead);
        }
        public ActorStatus Die()
        {
            return new ActorStatus(downed, true);
        }
        public ActorStatus UnDie()
        {
            return new ActorStatus(downed, false);
        }

    }


    public enum Stance
    {
        None, Physical, Mind, Energy, Chemical, Chaos, Nature, Super
    }
    [Flags]
    public enum BattleFlags
    {
        None, CharDowned
    }
    public enum TargetType
    {
       None, Self, SingleEnemy, SingleAlly, BurstAlly, BurstEnemy, AoeEnemy, AoeAlly, AoeAll
    }

    public class BattleEvent
    {
        public BattleEvent(Type type, int ActorID)
        {
            this.type = type;
            this.actorids = new List<int> { ActorID };
        }
        public enum Type
        {
            Down, Dead, Up, Res
        }
        public Type type;
        public List<int> actorids;
        public Func<GameState, GameState> execute = (gs) => { return gs; };
    }

    [Serializable]
    public class Actor
    {
        public string name;
        public CharStats stats;
        public int id;
        public float hp;
        public Stance stance;
        public ActorStatus status;
        // M means modified
        public int Matk(GameState gs) {/*?*/ return 0; }
        public int Mdef(GameState gs) {/*?*/ return 0; }
        public int Mspd(GameState gs) {/*?*/ return 0; }
        public ImmutableList<Buff> buffs;
        public Actor(string name, CharStats stats, int id, float hp, Stance stance, ActorStatus status, ImmutableList<Buff> buffs)
        {
            this.name = name;
            this.stats = stats;
            this.id = id;
            this.hp = hp;
            this.stance = stance;
            this.status = status;
            this.buffs = buffs;
        }

        public Actor TakeDmg(float damage)
        {
            if (status.downed)
            {
                damage *= 1.3f;
            }
            float newHP = hp - damage;
            if (newHP <= 0 && !status.dead) { 
                Debug.Log(name + " is Dead!");
                GameLoop.instance.EventStack(new BattleEvent(BattleEvent.Type.Dead, id));
                return new Actor(name, stats, id, newHP, stance, status.Die(), buffs);
            }
            return new Actor(name, stats, id, newHP, stance, status, buffs);  
        }
        public Actor HealDmg(float damage)
        {
            float newHP = hp + damage;
            newHP = MathF.Min(newHP, stats.Maxhp);
            Debug.Log("I smell healing... like " + damage + "hp to be exact");
            if (newHP > 0 && status.dead)
            {
                Debug.Log(name + " es eliveeeeee!!");
                GameLoop.instance.EventStack(new BattleEvent(BattleEvent.Type.Res, id));
                return new Actor(name, stats, id, newHP, stance, status.UnDie(), buffs);
            }
            return new Actor(name, stats, id, newHP, stance, status, buffs);
        }
        public Actor TakeStanceDmg(float damage, Stance stance, GameState gs, out BattleFlags flags) 
        {
            flags = BattleFlags.None;
            TypeCombo damagedTC = gs.FindCharWeakness(id);
            if (damagedTC.resistances.Contains(stance))
            {
                Debug.Log("It's not very effective...");
                damage *= 0.5f;
            }
            if (damagedTC.weaknesses.Contains(stance))
            {
                damage *= 1.5f;
                Debug.Log("It's super effective!!");

                if (!status.downed) 
                {
                    flags = BattleFlags.CharDowned;
                    GameLoop.instance.EventStack(new BattleEvent(BattleEvent.Type.Down, id));
                    Debug.Log(name + " was downed :O");
                }
                return TakeDmg(damage).WithStatus(status.Down());
            }
            return TakeDmg(damage);
        }



        public Actor WithStatus(ActorStatus status) 
        {
            return new Actor(name, stats, id, hp, stance, status, buffs);
        }
        public Actor WithBuff(Buff buff)
        {
            return new Actor(name, stats, id, hp, stance, status, buffs.Add(buff));
        }

        /*
        public Actor Buff(BuffStat buffStat, int duration) 
        {

        }
        */
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
        private BattleRandom random;
        
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
            this.random = new BattleRandom();
        }
        public GameState(
           ImmutableList<Actor> allies,
           ImmutableList<Actor> enemies,
           Actor currentActor,
           Stance currentStance,
           ImmutableDictionary<Stance, float> allyStancePoints,
           ImmutableDictionary<Stance, float> enemyStancePoints,
           BattleRandom random)
           
        {
            this.allies = allies;
            this.enemies = enemies;
            this.currentActor = currentActor;
            this.currentStance = currentStance;
            this.allyStancePoints = allyStancePoints;
            this.enemyStancePoints = enemyStancePoints;
            this.random = random;
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

        public GameState WithStance(Stance stance)
        {
            return new GameState(allies, enemies, currentActor, stance, allyStancePoints, enemyStancePoints, random);
        }

        public GameState SetCurrentActor(int id)
        {
            var newCurrentActor = allies.FirstOrDefault(a => a.id == id) ?? enemies.FirstOrDefault(a => a.id == id);
            if (newCurrentActor == null) 
            {
                Debug.LogError("Error in SetCurrentActor: Invalid ID :(");
                throw new ArgumentOutOfRangeException();
            }
            return new GameState(allies, enemies, newCurrentActor, currentStance, allyStancePoints, enemyStancePoints, random);
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
            return new GameState(Mallies, enemies, newCurrentActor, currentStance, allyStancePoints, enemyStancePoints, random);
        }
        public GameState WithEnemies(ImmutableList<Actor> Menemies)
        {
            var newCurrentActor = Menemies.FirstOrDefault(a => a.id == currentActor.id) ?? currentActor;
            return new GameState(allies, Menemies, newCurrentActor, currentStance, allyStancePoints, enemyStancePoints, random);
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
        
        public GameState GetRandom(float min, float max, out float value)
        {
            return new GameState(allies, enemies, currentActor, currentStance, allyStancePoints, enemyStancePoints, random.NextRandom(min, max, out value));
        }

        public GameState Copy()
        {
            return new GameState(allies, enemies, currentActor, currentStance, allyStancePoints, enemyStancePoints, random);
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
                case Stance.Energy: return Stance.Nature;
                case Stance.Nature: return Stance.Chemical;
                case Stance.Chemical: return Stance.Energy;
                case Stance.Physical: return Stance.Mind;
                case Stance.Mind: return Stance.Chaos;
                case Stance.Chaos: return Stance.Physical;
                default : return Stance.None;
            }
        }
        public static Stance FindResistance(Stance stance)
        {
            switch (stance)
            {
                case Stance.Energy: return Stance.Chemical;
                case Stance.Nature: return Stance.Energy;
                case Stance.Chemical: return Stance.Nature;
                case Stance.Physical: return Stance.Chaos;
                case Stance.Mind: return Stance.Physical;
                case Stance.Chaos: return Stance.Mind;
                default: return Stance.None;
            }
        }
        //Buff stages 2/4 3/4 4/4 5/4 6/4
        //            -2  -1   0   1   2
        // Basics: 1 or low 2 digit
        // Skill: Mid to high 2 digit
        // Nukes: Low 3 digit


        //Early game defense
        // 15 (10-20)
        //Late game defense
        // 50 (30-70)
        //Early game attack
        // 10 (8-12)
        //Late game attack
        // 60 (40-80)

        // Buffs last 3 cycles default

        public static int CalcDmg(Actor target, int power, Actor attacker, ref GameState gs)
        {
            float randomnumber;
            gs = gs.GetRandom(0.90f, 1.10f, out randomnumber);
            //Debug.LogError(randomnumber);
            int dmg = Mathf.RoundToInt(power * (attacker.stats.atk * StageToMulti(attacker.Matk(gs))/target.stats.def * StageToMulti(target.Mdef(gs))) * randomnumber);
            return dmg;
        }

        /* OLD
        public static List<float> GetMulti(int targetcount)
        {
            List<float> multis = new List<float>();
            for (int i = 0; i < targetcount; i++)
            {
                multis.Add(UnityEngine.Random.Range(0.95f, 1.05f));
            }
            return multis;
        }
        */
        public static float StageToMulti(int stage)
        {
            return (4 + (float)stage) / 4;
        }
       /* 
         public static float STM(int stage)
        {
            return StageToMulti(stage);
        }
       */

    }
    public class Buff
    {
        // Visual studio wanted me to make a singleton of this lol
        public Buff(int duration, IBuffEffect effect)
        {
            this.duration = duration;
            this.effect = effect;
        }
        int duration;
        IBuffEffect effect;
    }
    public interface IBuffEffect 
    {
        int GetAttack(GameState gs, int targetID);
        int GetDefense(GameState gs, int targetID);
        int GetSpeed(GameState gs, int targetID);
    }
}
