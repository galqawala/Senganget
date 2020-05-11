using System.Linq;
using LevelGenerator.Scripts.Helpers;
using UnityEngine;
using System.Collections.Generic;

namespace LevelGenerator.Scripts
{
    public class Section : MonoBehaviour
    {
        /// <summary>
        /// Section tags
        /// </summary>
        public string[] Tags;

        /// <summary>
        /// Tags that this section can annex
        /// </summary>
        public string[] CreatesTags;

        /// <summary>
        /// Exits node in hierarchy
        /// </summary>
        public Exits Exits;

        /// <summary>
        /// Bounds node in hierarchy
        /// </summary>
        public Bounds Bounds;

        /// <summary>
        /// Chances of the section spawning a dead end
        /// </summary>
        public int DeadEndChance;

        protected LevelGenerator LevelGenerator;
        protected int order;

        public class ExitPriority
        {
            public int Priority { get; set; }
            public Transform Exit { get; set; }
        }
        
        public void Initialize(LevelGenerator levelGenerator, int sourceOrder)
        {
            LevelGenerator = levelGenerator;
            transform.SetParent(LevelGenerator.Container);
            LevelGenerator.RegisterNewSection(this);
            order = sourceOrder + 1;
            this.name = this.name.Replace("(Clone)"," "+order);

            GenerateAnnexes();
        }

        protected void GenerateAnnexes()
        {
            var parentSectionName = this.name;

            string exitList = string.Join(",", Exits.ExitSpots.Select(e => e.name));

            //shuffle exits to process them in random (seeded) order
            var exitOrder = new List<ExitPriority>();
            foreach (var exit in Exits.ExitSpots)
            {
                exitOrder.Add(new ExitPriority{ 
                    Priority = RandomService.GetRandom(int.MinValue, int.MaxValue)
                ,   Exit = exit
                });

            }
            var shuffledExits = exitOrder.OrderBy(eo => eo.Priority);

            foreach (var exitPriority in shuffledExits)
            {
                var e = exitPriority.Exit;
                Debug.Log("'"+parentSectionName+"' exit '"+e.name+"' ("+exitList+") priority="+exitPriority.Priority);

                if (LevelGenerator.LevelSize > 0 && order < LevelGenerator.MaxAllowedOrder) {
                    if (RandomService.RollD100(DeadEndChance)) {
                        Debug.Log("DeadEndChance hit");
                        PlaceDeadEnd(e);
                    } else {
                        Debug.Log("GenerateSection(e);");
                        GenerateSection(e);
                    }
                } else {
                    Debug.Log("Limit hit --> dead end");
                    PlaceDeadEnd(e);
                }
            }
        }

        protected void GenerateSection(Transform exit)
        {
            if (CreatesTags.Any() && RandomService.GetRandom(0,2) == 0) {
                //prefab
                Debug.Log("Prefab section");
                var candidate = IsAdvancedExit(exit)
                    ? BuildSectionFromExit(exit.GetComponent<AdvancedExit>())
                    : BuildSectionFromExit(exit);
                    
                if (LevelGenerator.IsSectionValid(candidate.Bounds, Bounds))
                {
                    candidate.Initialize(LevelGenerator, order);
                }
                else
                {
                    Destroy(candidate.gameObject);
                    PlaceDeadEnd(exit);
                }
            } else {
                //dynamic
                Debug.Log("Dynamic section");
                //section
                GameObject sectionGameObject = (GameObject)Instantiate(Resources.Load("SectionTemplate"), exit.transform);
                sectionGameObject.name = "Dynamic section "+order;
                Section section = sectionGameObject.GetComponent<Section>();
                section.Initialize(LevelGenerator, order);
                //floor
                GameObject floorGameObject = (GameObject)Instantiate(LevelGenerator.floor, exit.transform);
                floorGameObject.transform.SetParent(sectionGameObject.transform);
                //left
                var rnd = RandomService.GetRandom(0,2);
                if (RandomService.GetRandom(0,2) == 0) {
                    //exit
                } else {
                    //wall
                }
            }
        }

        protected void PlaceDeadEnd(Transform exit) => Instantiate(LevelGenerator.DeadEnds.PickOne(), exit).Initialize(LevelGenerator);

        protected bool IsAdvancedExit(Transform exit) => exit.GetComponent<AdvancedExit>() != null;

        protected Section BuildSectionFromExit(Transform exit) => Instantiate(LevelGenerator.PickSectionWithTag(CreatesTags), exit).GetComponent<Section>();

        protected Section BuildSectionFromExit(AdvancedExit exit) => Instantiate(LevelGenerator.PickSectionWithTag(exit.CreatesTags), exit.transform).GetComponent<Section>();
    }
}