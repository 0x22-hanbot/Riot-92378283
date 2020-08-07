using System;
using System.Collections.Generic;
using System.Linq;
using SharpDX;
using System.Timers;
using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using EnsoulSharp.SDK.MenuUI.Values;
using EnsoulSharp.SDK.Prediction;
using EnsoulSharp.SDK.Utility;
using static P1_Katarina.Program;
namespace CHO_DEV
{
    class Program
    {

        static void Main(string[] args)
        {
            GameEvent.OnGameLoad += Loading_OnLoadingComplete;
        }



        private static AIHeroClient User => ObjectManager.Player;

        private static Spell Q;
        private static Spell W;
        private static Spell E;
        private static Spell R;
        private static List<Dagger> daggers = new List<Dagger>();


        private static int daggertime = 0;
        private static Vector3 previouspos;
        private static List<float> daggerstart = new List<float>();
        private static List<float> daggerend = new List<float>();
        public static List<Vector2> daggerpos = new List<Vector2>();
        public static Vector3 qdaggerpos;
        public static Vector3 wdaggerpos;
        public static int comboNum;





        public class Dagger
        {


            public float StartTime { get; set; }
            public float EndTime { get; set; }
            public Vector3 Position { get; set; }
            public int Width = 230;
        }

        //Declare the menu
        private static Menu KatarinaMenu, ComboMenu, LaneClearMenu, LastHitMenu, HarassAutoharass, DrawingsMenu, KillStealMenu, HumanizerMenu;


        //a list that contains Player spells
        private static List<Spell> SpellList = new List<Spell>();
        public static bool harassNeedToEBack = false;
        private static AIHeroClient target;

        private static bool HasRBuff()
        {
            var target = TargetSelector.GetTarget(E.Range, DamageType.Mixed);
            if (target == null) return false;
            else
            {
                return ObjectManager.Player.Spellbook.IsChanneling;
            }
        }
        public static float SpinDamage(AIBaseClient target)
        {
            return (float) ObjectManager.Player.CalculateDamage(target, DamageType.Magical, ((User.Level / 1.75f) + 3f) * User.Level + 71.5f + 1.25f * (ObjectManager.Player.TotalAttackDamage - ObjectManager.Player.BaseAttackDamage) + User.TotalMagicalDamage * new[] { .55f, .70f, .80f, 1.00f }[R.Level]);

        }
        public static float QDamage(AIBaseClient target)
        {
            if (Q.IsReady())
                return (float)ObjectManager.Player.CalculateDamage(target, DamageType.Magical, new[] { 0f, 75f, 105f, 135f, 165f, 195f }[Q.Level] + 0.3f * ObjectManager.Player.TotalMagicalDamage);
            else
                return 0f;
        }
        public static float WDamage(AIBaseClient target)
        {
            return 0f;
        }
        public static float EDamage(AIBaseClient target)
        {
            if (E.IsReady())
                return (float)ObjectManager.Player.CalculateDamage(target, DamageType.Magical, new[] { 0f, 25f, 40f, 55f, 70f, 85f }[E.Level] + 0.25f * ObjectManager.Player.TotalMagicalDamage + 0.5f * User.TotalAttackDamage);
            else
                return 0f;
        }
        public static float RDamage(AIBaseClient target)
        {
            if (R.IsReady())
                return (float)ObjectManager.Player.CalculateDamage(target, DamageType.Magical, (new[] { 0f, 375f, 562.5f, 750f }[R.Level] + 2.85f * ObjectManager.Player.TotalMagicalDamage + 3.3f * (ObjectManager.Player.TotalAttackDamage - ObjectManager.Player.BaseAttackDamage)));
            else
                return 0f;
        }

        private static void Loading_OnLoadingComplete()
        {

            //Makes sure you are Katarina fdsgfdgdsgsd
            if (User.CharacterName != "Katarina")
                return;

            //print("P1 Katarina loaded! Have fun!");
            //Creates the menu
            KatarinaMenu = new Menu("Katarina", "P1 Katarina",true);

            //Creates a SubMenu
            ComboMenu = KatarinaMenu.Add(new Menu("Combo", "Combo"));
            LaneClearMenu = KatarinaMenu.Add(new Menu("LaneClear", "LaneClear"));
            LastHitMenu = KatarinaMenu.Add(new Menu("LastHit", "LastHit"));
            HarassAutoharass = KatarinaMenu.Add(new Menu("HarassAutoHarass", "Harass / Auto Harass"));
            KillStealMenu = KatarinaMenu.Add(new Menu("KillSteal", "KillSteal"));

            HumanizerMenu = KatarinaMenu.Add(new Menu("Humanizer", "Humanizer"));
            DrawingsMenu = KatarinaMenu.Add(new Menu("Drawings", "Drawings"));

            //Checkbox should be - YourMenu.Add(String MenuID, new CheckBox(String DisplayName, bool DefaultValue);
            //ComboMenu.AddLabel("I don't know what to have here, if you have any suggestions please tell me");
            ComboMenu.Add( new MenuBool("ComboEAA", "Only use e if target is outside auto attack range"));
            LaneClearMenu.Add( new MenuBool("LaneClearQ", "Use Q in lane clear"));
            LastHitMenu.Add( new MenuBool("LastHitQ", "Use Q in last hit"));
            HarassAutoharass.Add( new MenuBool("HQ", "Use Q in harass"));
            HarassAutoharass.Add( new MenuBool("CC", "Use E reset combo in harass"));
            HarassAutoharass.Add( new MenuBool("AHQ", "Use Q in auto harass"));
            KillStealMenu.Add( new MenuBool("KillStealQ", "Use Q to killsteal"));
            KillStealMenu.Add(new MenuBool("KillStealE", "Use E to killsteal"));
            KillStealMenu.Add(new MenuBool("KillStealEW", "Use EW to killsteal"));
            KillStealMenu.Add( new MenuBool("KillStealR", "Use R to killsteal", false));
            HumanizerMenu.Add( new MenuSlider("HumanizerQ", "Q delay", 0, 0, 1000));
            HumanizerMenu.Add( new MenuSlider("HumanizerW", "W delay", 0, 0, 1000));
            HumanizerMenu.Add( new MenuSlider("HumanizerE", "E delay", 0, 0, 1000));
            HumanizerMenu.Add( new MenuSlider("HumanizerR", "R delay", 0, 0, 1000));

            KatarinaMenu.Attach();

            //Giving Q values
            Q = new Spell(SpellSlot.Q, 600);

            //Giving W values
            W = new Spell(SpellSlot.W, 150);

            //Giving E values
            E = new Spell(SpellSlot.E, 700);
            E.SetSkillshot(7,150,float.MaxValue,false,SkillshotType.Circle);
            //Giving R values
            R = new Spell(SpellSlot.R, 550);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);

            //Creating menu using foreach from a list
            foreach (var Spell in SpellList)
            {
                //Creates checkboxes using Spell Slot
                DrawingsMenu.Add( new MenuBool(Spell.Slot.ToString(), "Draw " + Spell.Slot));
            }


            //used for drawings that dont override game UI
            Drawing.OnDraw += Drawing_OnDraw;
            Drawing.OnEndScene += Damage_Indicator;
            //Drawing.OnEndScene += Draw_Q;


            //happens on every core tick
            Game.OnUpdate += Game_OnTick;
            Game.OnUpdate += Game_OnTick1;
        }

        private static void Game_OnTick1(EventArgs args)
        {
            if (HarassAutoharass["AHQ"].GetValue<MenuBool>().Enabled)
            {
                target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
                if(target != null)
                castQ(target);
            }
            target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
            if (target != null && QDamage(target) >= target.Health && KillStealMenu["KillStealQ"].GetValue<MenuBool>().Enabled)
                castQ(target);
            target = TargetSelector.GetTarget(W.Range, DamageType.Magical);
            if (target != null && WDamage(target) >= target.Health && KillStealMenu["KillStealW"].GetValue<MenuBool>().Enabled)
                CastW();
            target = TargetSelector.GetTarget(E.Range, DamageType.Magical);
            if (target != null && EDamage(target) >= target.Health && KillStealMenu["KillStealE"].GetValue<MenuBool>().Enabled)
                CastE(target.Position);
            target = TargetSelector.GetTarget(E.Range, DamageType.Magical);
            if (target != null && EDamage(target) + WDamage(target) >= target.Health && KillStealMenu["KillStealEW"].GetValue<MenuBool>().Enabled)
            {
                CastE(target.Position);
                CastW();
            }
        }

        public static void castQ(AIBaseClient target)
        {
            if(target == null)return;
            Q.Cast(target);

            // daggers.Add(new Dagger() { StartTime = Game.Time + 2, EndTime = Game.Time + 7, Position = ObjectManager.Get<AIMinionClient>().LastOrDefault(a => a.Name == "HiddenMinion" && a.IsValid).Position });

            qdaggerpos = ObjectManager.Get<AIMinionClient>().LastOrDefault(a => a.Name == "HiddenMinion" && a.IsValid).Position;


        }
        private static void CastW()
        {

            W.Cast();



            //daggers.Add(new Dagger() { StartTime = Game.Time + 1.25f, EndTime = Game.Time + 6.25f, Position = ObjectManager.Get<AIMinionClient>().LastOrDefault(a => a.Name == "HiddenMinion" && a.IsValid).Position });

            wdaggerpos = User.Position;
        }
        private static void CastE(Vector3 target)
        {
            if (daggers.Count == 0 && !HasRBuff())
                E.Cast(target);
            foreach (Dagger dagger in daggers)
            {

                {

                }
                if (target.Distance(dagger.Position) <= 550)
                    User.Spellbook.CastSpell(E.Slot, dagger.Position.Extend(target, 150));

                else if (ComboMenu["ComboEAA"].GetValue<MenuBool>().Enabled && target.Distance(User) >= User.GetRealAutoAttackRange(null))
                    E.Cast(target);
                else if (!ComboMenu["ComboEAA"].GetValue<MenuBool>().Enabled)
                    E.Cast(target);
                else
                    return;
            }





        }
        private static void Game_OnTick(EventArgs args)
        {



//            var target = TargetSelector.GetTarget(E.Range, DamageType.Magical);
//
//            target = TargetSelector.GetTarget(E.Range, DamageType.Magical);
            // //print(target.Direction);
            // else if(!target.IsFacing(User))
            // {
            //  //print("no");
            // }

            if (HasRBuff())
            {
                Orbwalker.MovementState = false;
                Orbwalker.AttackState = false;
            }
            else
            {
                Orbwalker.MovementState = true;
                Orbwalker.AttackState = true;
            }
            if (Orbwalker.ActiveMode == OrbwalkerMode.LaneClear)
            {
                LaneClear();
            }
            if (Orbwalker.ActiveMode == OrbwalkerMode.LastHit)
            {

                LastHit();

            }
            if (Orbwalker.ActiveMode == OrbwalkerMode.Combo)
            {
                Combo();
            }
            if (Orbwalker.ActiveMode == OrbwalkerMode.Harass)
            {
                Harass();
            }
            if (Orbwalker.ActiveMode != OrbwalkerMode.Combo)
            {
                Orbwalker.AttackState = true;
            }






            for (var index = daggers.Count - 1; index >= 0; index--)
            {

                ////print("dagger: " + daggers[index].EndTime);

                if (User.Distance(daggers[index].Position) <= daggers[index].Width && Game.Time >= daggers[index].StartTime || daggers[index] == null || Game.Time >= daggers[index].EndTime)
                {
                    daggers.RemoveAt(index);

                }
            }

            // kills = User.ChampionsKilled;
            //assists = User.Assists;

            // if(target.IsFacing(User))



            var DaggerFirst = ObjectManager.Get<AIMinionClient>().LastOrDefault(a => a.Name == "HiddenMinion" && a.IsValid).Position;


            if (DaggerFirst != null && ObjectManager.Get<AIMinionClient>().LastOrDefault(a => a.Name == "HiddenMinion" && a.IsValid).Position != previouspos)
            {
                //print("Added dagger");
                daggers.Add(new Dagger() { StartTime = Game.Time + 1.25f, EndTime = Game.Time + 5.1f, Position = ObjectManager.Get<AIMinionClient>().LastOrDefault(a => a.Name == "HiddenMinion" && a.IsValid).Position });
                previouspos = ObjectManager.Get<AIMinionClient>().LastOrDefault(a => a.Name == "HiddenMinion" && a.IsValid).Position;
            }
        }
        private static void Harass()
        {

            if (HarassAutoharass["HQ"].GetValue<MenuBool>().Enabled && Q.IsReady())
            {
                var target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
                if (target.IsValidTarget())
                    castQ(target);
            }
            if (HarassAutoharass["CC"].GetValue<MenuBool>().Enabled)
            {
                if (harassNeedToEBack && E.IsReady())
                {
                    User.Spellbook.CastSpell(E.Slot, wdaggerpos);
                    harassNeedToEBack = false;
                }


                target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
                if (target.IsValidTarget() && !harassNeedToEBack)
                {
                    DelayAction.Add(HumanizerMenu["HumanizerQ"].GetValue<MenuSlider>().Value, () => castQ(target));
                    DelayAction.Add(HumanizerMenu["HumanizerW"].GetValue<MenuSlider>().Value + 50, () => CastW());
                    DelayAction.Add(HumanizerMenu["HumanizerE"].GetValue<MenuSlider>().Value + 250, () => CastE(target.Position));
                    if (!E.IsReady())
                        harassNeedToEBack = true;


                }

            }
        }
        private static void LaneClear()
        {
            var minions = GameObjects.EnemyMinions.Where(a => a.Distance(ObjectManager.Player) < Q.Range).OrderBy(a => a.Health);
            var minion = minions.FirstOrDefault();
            if (minion == null) return;

            if (LaneClearMenu["LaneClearQ"].GetValue<MenuBool>().Enabled && (QDamage(minion) > minion.Health) && Q.IsReady())
            {
                Program.castQ(minion);
            }

        }
        private static void LastHit()
        {

            var minions = GameObjects.EnemyMinions.Where(x => x.Distance(ObjectManager.Player) < Q.Range).OrderBy(a => a.Health);
            var minion = minions.FirstOrDefault();
            //print(minion);
            if (!minion.IsValidTarget())
                return;
            if (LastHitMenu["LastHitQ"].GetValue<MenuBool>().Enabled && (QDamage(minion) >= minion.Health) && Q.IsReady())
            {
                castQ(minion);
            }

        }

        private static void Combo()
        {
            var target = TargetSelector.GetTarget(E.Range, DamageType.Magical);
            if (E.IsReady() && Q.IsReady() && W.IsReady() && comboNum == 0)
            {
                if (!HasRBuff() || (HasRBuff() && target.Health < QDamage(target) + WDamage(target) + EDamage(target) + (2f * SpinDamage(target))))
                    comboNum = 1;
            }

            else if (E.IsReady() && Q.IsReady() && comboNum == 0)
            {
                if (!HasRBuff() || (HasRBuff() && target.Health < QDamage(target) + EDamage(target) + SpinDamage(target)))
                    comboNum = 2;
            }

            else if (W.IsReady() && E.IsReady() && comboNum == 0)
            {
                if (!HasRBuff() || (HasRBuff() && target.Health < EDamage(target) + SpinDamage(target)))
                    comboNum = 3;
            }

            else if (E.IsReady() && comboNum == 0)
            {
                if (!HasRBuff() || (HasRBuff() && target.Health < EDamage(target)))
                    comboNum = 4;
            }

            else if (Q.IsReady() && comboNum == 0)
            {
                if (!HasRBuff() || (HasRBuff() && target.Health < QDamage(target)))
                    comboNum = 5;
            }

            else if (W.IsReady() && comboNum == 0 && User.Distance(target) <= 300)
            {
                if (!HasRBuff())
                    comboNum = 6;
            }

            else if (R.IsReady() && comboNum == 0 && User.Distance(target)<=400)
                comboNum = 7;

            //combo 1, Q W and E
            if (comboNum == 1)
            {
                castQ(target);
                DelayAction.Add(0, () => CastE(User.Position.Extend(target.Position, User.Distance(target) + 140)));

                DelayAction.Add(50, () => CastW());


                if (!Q.IsReady() && !W.IsReady() && !E.IsReady())
                    comboNum = 0;
            }

            //combo 2, Q and E
            if (comboNum == 2)
            {
                castQ(target);
                DelayAction.Add(0, () => CastE(User.Position.Extend(target.Position, User.Distance(target) + 140)));

                if (!Q.IsReady() && !E.IsReady())
                    comboNum = 0;
            }

            //combo 3, W and E
            if (comboNum == 3)
            {
                DelayAction.Add(0, () => CastE(User.Position.Extend(target.Position, User.Distance(target) + 140)));
                DelayAction.Add(50, () => CastW());

                if (!W.IsReady() && !E.IsReady())
                    comboNum = 0;
            }

            //combo 4, E
            if (comboNum == 4)
            {
                CastE(target.Position);
                comboNum = 0;
            }

            //combo 5, Q
            if (comboNum == 5)
            {
                castQ(target);
                comboNum = 0;
            }

            //combo 6, W
            if (comboNum == 6)
            {
                CastW();
                comboNum = 0;
            }

            //combo 7, R
            if (comboNum == 7)
            {
                R.Cast();
                comboNum = 0;
            }

        }
        private static void Drawing_OnDraw(EventArgs args)
        {
            foreach (Dagger dagger in daggers)
            {
                if (dagger.StartTime <= Game.Time)
                {
                    Drawing.DrawCircle(dagger.Position, 140, System.Drawing.Color.SandyBrown);
                }
                else
                    Drawing.DrawCircle(dagger.Position,140, System.Drawing.Color.Red );


            }
            var DaggerFirst = ObjectManager.Get<AIMinionClient>().LastOrDefault(a => a.Name == "HiddenMinion" && a.IsValid);
            var DaggerLast = ObjectManager.Get<AIMinionClient>().LastOrDefault(a => a.Name == "HiddenMinion" && a.IsValid);
            //returns Each spell from the list that are enabled from the menu

            //Circle.Draw(Color.Green, 150, ObjectManager.Get<AIMinionClient>().LastOrDefault(a => a.Name == "HiddenMinion" && a.IsValid));

            foreach (var Spell in SpellList.Where(Spell => DrawingsMenu[Spell.Slot.ToString()].GetValue<MenuBool>().Enabled))
            {

                //Draws a circle with spell range around the player
                //Circle.Draw(Color.Green, 150, DaggerFirst.Position);
                //Circle.Draw(Color.Green, 150, DaggerLast.Position);
                Drawing.DrawCircle(User.Position,Spell.Range, Spell.IsReady() ? System.Drawing.Color.SteelBlue : System.Drawing.Color.OrangeRed );
            }

        }
        private static void Damage_Indicator(EventArgs args)
        {

            foreach (var unit in GameObjects.EnemyHeroes.Where(x => x.IsVisibleOnScreen && x.IsValidTarget() && x.IsHPBarRendered))
            {
                var damage = 0f;
                if (Q.IsReady() && W.IsReady() && E.IsReady())
                    damage = QDamage(unit) + WDamage(unit) + EDamage(unit) + (2f * SpinDamage(unit));
                else if (Q.IsReady() && W.IsReady())
                    damage = QDamage(unit) + WDamage(unit) + SpinDamage(unit);
                else if (Q.IsReady() && E.IsReady())
                    damage = QDamage(unit) + EDamage(unit) + SpinDamage(unit);
                else if (W.IsReady() && E.IsReady())
                    damage = EDamage(unit) + WDamage(unit) + SpinDamage(unit);
                else if (Q.IsReady())
                    damage = QDamage(unit);
                else if (W.IsReady())
                    damage = WDamage(unit);
                else if (E.IsReady())
                    damage = EDamage(unit);
                if (R.IsReady())
                    damage += RDamage(unit);
                //Chat.Print(damage);
                var Special_X = unit.CharacterName == "Jhin" || unit.CharacterName == "Annie" ? -12 : 0;
                var Special_Y = unit.CharacterName == "Jhin" || unit.CharacterName == "Annie" ? -3 : 9;

                var DamagePercent = ((unit.Health - damage) > 0
                    ? (unit.Health - damage)
                    : 0) / (unit.MaxHealth + unit.AllShield + unit.PhysicalShield + unit.MagicalShield);
                var currentHealthPercent = unit.Health / (unit.MaxHealth + unit.AllShield + unit.PhysicalShield + unit.MagicalShield);
                var StartPoint = new Vector2((int)(unit.HPBarPosition.X + DamagePercent * 107), (int)unit.HPBarPosition.Y - 5 + 14);
                var EndPoint = new Vector2((int)(unit.HPBarPosition.X + currentHealthPercent * 107) + 1, (int)unit.HPBarPosition.Y - 5 + 14);

                Drawing.DrawLine(StartPoint, EndPoint, 9.82f, System.Drawing.Color.SandyBrown);

            }
        }
    }
}
