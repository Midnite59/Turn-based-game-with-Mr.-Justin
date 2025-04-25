using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using UnityEngine;
using System.Linq;
using UnityEditor.SceneManagement;
using UnityEngine.Assertions.Must;
using System.Xml.Linq;

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
        public int Matk(GameState gs) {/*?*/ return buffs.Aggregate(0, (total, next) => total += next.effect.GetAttack(gs, id), (total) => Mathf.Clamp(total, -2, 2)); }
        public int Mdef(GameState gs) {/*?*/ return buffs.Aggregate(0, (total, next) => total += next.effect.GetDefense(gs, id), (total) => Mathf.Clamp(total, -2, 2)); }
        public int Mspd(GameState gs) {/*?*/ return buffs.Aggregate(0, (total, next) => total += next.effect.GetSpeed(gs, id), (total) => Mathf.Clamp(total, -2, 2)); }
        public ImmutableList<Buff> buffs;
        public List<Buff> visibleBuffs;
        public Actor(string name, CharStats stats, int id, float hp, Stance stance, ActorStatus status, ImmutableList<Buff> buffs)
        {
            this.name = name;
            this.stats = stats;
            this.id = id;
            this.hp = hp;
            this.stance = stance;
            this.status = status;
            this.buffs = buffs;
            this.visibleBuffs = buffs.ToList();
        }

        public Actor TakeDmg(float damage, BattleFlags flags)
        {
            ActorStatus newStatus = status;

            if (status.downed)
            {
                damage *= 1.3f;
            }
            if ((flags & BattleFlags.CharDowned) == BattleFlags.CharDowned)
            {
                newStatus = newStatus.Down();
            }
            float newHP = hp - damage;
            if (newHP <= 0 && !status.dead) { 
                Debug.Log(name + " is Dead!");
                GameLoop.instance.EventStack(new BattleEvent(BattleEvent.Type.Dead, id));
                newStatus = newStatus.Die();
            }
            return new Actor(name, stats, id, newHP, stance, newStatus, buffs);  
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



        public Actor WithStatus(ActorStatus status) 
        {
            return new Actor(name, stats, id, hp, stance, status, buffs);
        }
        public Actor WithBuff(Buff buff)
        {
            if (buffs.Any(b => b.buffID == buff.buffID))
            {
                return new Actor(name, stats, id, hp, stance, status, buffs.Select(b => b.buffID == buff.buffID ? new Buff(buff.duration, b.effect, b.uid):b).ToImmutableList());
            }
            else
            {
                return new Actor(name, stats, id, hp, stance, status, buffs.Add(buff));
            }
        }

        public Actor TickBuffs()
        {
            return new Actor(name, stats, id, hp, stance, status, buffs.Select(b => b.TickDown()).Where(b => b.duration > 0).ToImmutableList());
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
        public float allyStancePoints;
        public float enemyStancePoints;
        public IEnumerable<Actor> actors
        { get {  return allies.Concat(enemies); } }
        private BattleRandom random;
        
        //public ImmutabeList<Action> effects;

        public GameState(
            ImmutableList<Actor> allies, 
            ImmutableList<Actor> enemies, 
            Actor currentActor, 
            Stance currentStance, 
            float allyStancePoints, 
            float enemyStancePoints) 
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
           float allyStancePoints,
           float enemyStancePoints,
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
            Stance cres = Helper.FindResistance(charStance);
            Stance sres = Helper.FindResistance(stateStance);
            if (StanceWeaks.Contains(cres))
            {
                StanceWeaks.Remove(cres);
            }
            else
            {
                StanceRes.Add(cres);
            }
            if (StanceWeaks.Contains(sres))
            {
                StanceWeaks.Remove(sres);
            }
            else
            {
                StanceRes.Add(sres);
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

        public GameState TickDownBuffs()
        {
            return actors.Aggregate(this, (gs, actor) => gs.WithActor(actor.TickBuffs()));
        }

        public GameState RegainSP()
        {
            BattleRandom rand = random;
            float enemySP = enemyStancePoints;
            float allySP = allies.Aggregate(allyStancePoints, (sp, actor) => sp + actor.stats.eff >= 6 ? sp : sp + actor.stats.eff);
            // Skill point blackjack
            return new GameState(allies, enemies, currentActor, currentStance, allySP, enemySP, rand);
        }
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

        public static int CalcDmg(Actor target, int power, Actor attacker, ref GameState gs, out BattleFlags flags, Stance? stanceOverride = null)
        {
            float randomnumber;
            gs = gs.GetRandom(0.90f, 1.10f, out randomnumber);
            //Debug.LogError(randomnumber);
            float dmg = Mathf.Round(power * (attacker.stats.atk * StageToMulti(attacker.Matk(gs))/(target.stats.def * StageToMulti(target.Mdef(gs)))) * randomnumber);
            

            // Stancy stuff
            Stance stance = stanceOverride ?? attacker.stance;
            flags = BattleFlags.None;
            TypeCombo damagedTC = gs.FindCharWeakness(target.id);
            if (damagedTC.resistances.Contains(stance))
            {
                //Debug.Log("It's not very effective...");
                dmg *= 0.5f;
            }
            if (damagedTC.weaknesses.Contains(stance))
            {
                dmg *= 1.5f;
                //Debug.Log("It's super effective!!");

                if (!target.status.downed)
                {
                    flags = BattleFlags.CharDowned;
                    GameLoop.instance.EventStack(new BattleEvent(BattleEvent.Type.Down, target.id));
                    //Debug.Log(target.name + " was downed :O");
                }
            }
            return Mathf.RoundToInt(dmg);
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
    [Serializable]
    public class Buff
    {
        // Visual studio wanted me to make a singleton of this lol
        public Buff(int duration, IBuffEffect effect, int uid)
        {
            this.duration = duration;
            this.effect = effect;
            this.buffID = effect.GetHashCode() * 100 + uid;
            this.uid = uid;
        }
        public int duration;
        public int buffID;
        public int uid;
        public IBuffEffect effect;

        public Buff TickDown()
        {
            return new Buff(this.duration - 1, this.effect, this.uid);
        }
    }
    public interface IBuffEffect 
    {
        public int GetAttack(GameState gs, int targetID);
        public int GetDefense(GameState gs, int targetID);
        public int GetSpeed(GameState gs, int targetID);
    }
}
