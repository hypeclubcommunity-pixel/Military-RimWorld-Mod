using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Military
{
    public class Window_SquadManager : Window
    {
        private string selectedSquadId;
        private Vector2 leftScroll = Vector2.zero;
        private Vector2 rightScroll = Vector2.zero;

        public override Vector2 InitialSize => new Vector2(900f, 600f);

        public Window_SquadManager()
        {
            doCloseX = true;
            draggable = true;
            resizeable = true;
            absorbInputAroundWindow = false;
        }

        public override void DoWindowContents(Rect inRect)
        {
            GameComponent_MilitaryManager manager = GameComponent_MilitaryManager.Instance;
            if (manager == null)
            {
                Widgets.Label(inRect, "Military manager not found.");
                return;
            }

            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(inRect.x, inRect.y, inRect.width, 36f), "Military Squads");
            Text.Font = GameFont.Small;

            Rect contentRect = new Rect(inRect.x, inRect.y + 40f, inRect.width, inRect.height - 40f);
            Rect leftRect = new Rect(contentRect.x, contentRect.y, 300f, contentRect.height);
            Rect rightRect = new Rect(leftRect.xMax + 12f, contentRect.y, contentRect.width - leftRect.width - 12f, contentRect.height);

            DrawLeftPanel(leftRect, manager);
            DrawRightPanel(rightRect, manager);
        }

        private void DrawLeftPanel(Rect rect, GameComponent_MilitaryManager manager)
        {
            Widgets.DrawMenuSection(rect);

            Rect listRect = rect.ContractedBy(8f);
            float buttonsHeight = 35f;
            Rect outRect = new Rect(listRect.x, listRect.y, listRect.width, listRect.height - buttonsHeight - 8f);

            float rowHeight = 34f;
            float viewHeight = Mathf.Max(1f, manager.squads.Count * rowHeight);
            Rect viewRect = new Rect(0f, 0f, outRect.width - 16f, viewHeight);

            Widgets.BeginScrollView(outRect, ref leftScroll, viewRect);

            EnsureValidSelection(manager);

            for (int i = 0; i < manager.squads.Count; i++)
            {
                SquadData squad = manager.squads[i];
                Rect row = new Rect(0f, i * rowHeight, viewRect.width, rowHeight - 2f);
                bool selected = squad != null && squad.squadId == selectedSquadId;

                if (selected)
                    Widgets.DrawHighlightSelected(row);
                else if (Mouse.IsOver(row))
                    Widgets.DrawHighlight(row);

                if (Widgets.ButtonInvisible(row))
                    selectedSquadId = squad?.squadId;

                if (squad == null)
                    continue;

                Map map = ResolveMap();
                Pawn leader = squad.GetLeader(map);
                string leaderName = leader != null ? leader.LabelShort : "No leader";
                int memberCount = squad.memberPawnIds?.Count ?? 0;
                string memberLabel = memberCount == 1 ? "member" : "members";

                Widgets.Label(row.ContractedBy(4f), $"{squad.squadName} / {leaderName} / {memberCount} {memberLabel}");
            }

            Widgets.EndScrollView();

            float buttonY = rect.yMax - buttonsHeight - 6f;
            Rect createRect = new Rect(rect.x + 8f, buttonY, 136f, 30f);
            Rect disbandRect = new Rect(createRect.xMax + 8f, buttonY, 136f, 30f);

            if (Widgets.ButtonText(createRect, "+ Create Squad"))
            {
                Find.WindowStack.Add(new Dialog_CreateSquad(created =>
                {
                    if (created != null)
                        selectedSquadId = created.squadId;
                }));
            }

            SquadData selectedSquad = manager.GetSquadById(selectedSquadId);
            if (selectedSquad == null)
                GUI.color = Color.gray;

            if (Widgets.ButtonText(disbandRect, "Disband") && selectedSquad != null)
            {
                manager.DisbandSquad(selectedSquad.squadId);
                selectedSquadId = null;
            }
            GUI.color = Color.white;
        }

        private void DrawRightPanel(Rect rect, GameComponent_MilitaryManager manager)
        {
            Widgets.DrawMenuSection(rect);

            EnsureValidSelection(manager);
            SquadData squad = manager.GetSquadById(selectedSquadId);
            if (squad == null)
            {
                Widgets.Label(rect.ContractedBy(10f), "Select a squad from the left panel.");
                return;
            }

            Rect outRect = rect.ContractedBy(8f);
            Rect viewRect = new Rect(0f, 0f, outRect.width - 16f, 800f);
            Widgets.BeginScrollView(outRect, ref rightScroll, viewRect);

            Map map = ResolveMap();
            float curY = 0f;

            Widgets.Label(new Rect(0f, curY, 100f, 28f), "Name:");
            squad.squadName = Widgets.TextField(new Rect(104f, curY, 260f, 28f), squad.squadName ?? "");
            curY += 42f;

            Pawn leader = squad.GetLeader(map);
            if (leader == null)
            {
                GUI.color = Color.red;
                Widgets.Label(new Rect(0f, curY, viewRect.width, 28f), "No eligible leader \u2014 assign a Sergeant or Lieutenant");
                GUI.color = Color.white;
                curY += 34f;

                if (Widgets.ButtonText(new Rect(0f, curY, 150f, 28f), "Assign Leader"))
                {
                    List<FloatMenuOption> leaderOptions = BuildAssignLeaderOptions(manager, squad);
                    if (leaderOptions.Count == 0)
                        leaderOptions.Add(new FloatMenuOption("No eligible leaders", null));
                    Find.WindowStack.Add(new FloatMenu(leaderOptions));
                }
                curY += 34f;
            }
            else
            {
                DrawPawnRow(new Rect(0f, curY, viewRect.width, 48f), leader, isLeader: true, onRemove: null);
                curY += 56f;
            }

            Widgets.Label(new Rect(0f, curY, viewRect.width, 28f), "Members");
            curY += 30f;

            List<Pawn> members = squad.GetMembers(map);
            for (int i = 0; i < members.Count; i++)
            {
                Pawn member = members[i];
                DrawPawnRow(new Rect(0f, curY, viewRect.width, 48f), member, isLeader: false, onRemove: () => manager.RemoveMember(squad.squadId, member));
                curY += 54f;
            }

            for (int i = members.Count; i < SquadData.MaxMembers; i++)
            {
                Rect emptyRow = new Rect(0f, curY, viewRect.width, 44f);
                Widgets.DrawHighlight(emptyRow);
                GUI.color = Color.gray;
                Widgets.Label(emptyRow.ContractedBy(8f), "Empty slot");
                GUI.color = Color.white;
                curY += 50f;
            }

            Rect addRect = new Rect(0f, curY + 8f, 140f, 30f);
            bool canAdd = members.Count < SquadData.MaxMembers;
            if (!canAdd)
                GUI.color = Color.gray;

            if (Widgets.ButtonText(addRect, "+ Add Member") && canAdd)
            {
                List<FloatMenuOption> options = BuildAddMemberOptions(manager, squad);
                if (options.Count == 0)
                    options.Add(new FloatMenuOption("No eligible colonists", null));
                Find.WindowStack.Add(new FloatMenu(options));
            }
            GUI.color = Color.white;

            Widgets.EndScrollView();
        }

        private List<FloatMenuOption> BuildAssignLeaderOptions(GameComponent_MilitaryManager manager, SquadData squad)
        {
            List<FloatMenuOption> options = new List<FloatMenuOption>();
            List<Pawn> pawns = PawnsFinder.AllMaps_FreeColonists.ToList();

            List<Pawn> candidates = new List<Pawn>();
            for (int i = 0; i < pawns.Count; i++)
            {
                Pawn pawn = pawns[i];
                if (!SquadData.IsValidLeader(pawn))
                    continue;
                if (!MilitaryUtility.IsEligible(pawn))
                    continue;

                SquadData existing = manager.GetSquadOf(pawn);
                if (existing != null && existing.leaderPawnId == pawn.thingIDNumber)
                    continue;

                candidates.Add(pawn);
            }

            bool hasLieutenant = false;
            for (int i = 0; i < candidates.Count; i++)
            {
                MilitaryStatComp lComp = MilitaryUtility.GetComp(candidates[i]);
                if (lComp != null && lComp.rank == "Lieutenant")
                {
                    hasLieutenant = true;
                    break;
                }
            }

            for (int i = 0; i < candidates.Count; i++)
            {
                Pawn candidate = candidates[i];
                MilitaryStatComp comp = MilitaryUtility.GetComp(candidate);
                if (hasLieutenant && (comp == null || comp.rank != "Lieutenant"))
                    continue;

                Pawn captured = candidate;
                string label = $"{captured.LabelShort} ({comp?.rank})";
                options.Add(new FloatMenuOption(label, () =>
                {
                    // Clear old leader's comp data if still set
                    if (squad.leaderPawnId != -1)
                    {
                        Pawn oldLeader = null;
                        foreach (Map m in Find.Maps)
                        {
                            oldLeader = m.mapPawns.AllPawns.FirstOrDefault(p => p.thingIDNumber == squad.leaderPawnId);
                            if (oldLeader != null)
                                break;
                        }
                        if (oldLeader != null)
                        {
                            MilitaryStatComp oldComp = MilitaryUtility.GetComp(oldLeader);
                            if (oldComp != null)
                            {
                                oldComp.squadId = "";
                                oldComp.isSquadLeader = false;
                            }
                        }
                    }

                    // Remove from current squad if member somewhere
                    SquadData prevSquad = manager.GetSquadOf(captured);
                    if (prevSquad != null)
                        manager.RemoveMember(prevSquad.squadId, captured);

                    squad.leaderPawnId = captured.thingIDNumber;
                    MilitaryStatComp c = MilitaryUtility.GetComp(captured);
                    if (c != null)
                    {
                        c.squadId = squad.squadId;
                        c.isSquadLeader = true;
                    }
                }));
            }

            return options;
        }

        private List<FloatMenuOption> BuildAddMemberOptions(GameComponent_MilitaryManager manager, SquadData squad)
        {
            List<FloatMenuOption> options = new List<FloatMenuOption>();
            List<Pawn> pawns = PawnsFinder.AllMaps_FreeColonists.ToList();

            for (int i = 0; i < pawns.Count; i++)
            {
                Pawn pawn = pawns[i];
                if (pawn == null || pawn.Faction != Faction.OfPlayer)
                    continue;

                if (!MilitaryUtility.IsEligible(pawn))
                    continue;

                MilitaryStatComp comp = MilitaryUtility.GetComp(pawn);
                if (comp == null || string.IsNullOrEmpty(comp.rank))
                    continue;

                if (manager.GetSquadOf(pawn) != null)
                    continue;

                Pawn captured = pawn;
                string label = $"{captured.LabelShort} ({comp.rank})";
                options.Add(new FloatMenuOption(label, () => manager.AddMember(squad.squadId, captured)));
            }

            return options;
        }

        private static Map ResolveMap()
        {
            if (Find.CurrentMap != null)
                return Find.CurrentMap;

            if (Find.Maps != null && Find.Maps.Count > 0)
                return Find.Maps[0];

            return null;
        }

        private void EnsureValidSelection(GameComponent_MilitaryManager manager)
        {
            if (manager.squads == null || manager.squads.Count == 0)
            {
                selectedSquadId = null;
                return;
            }

            if (string.IsNullOrEmpty(selectedSquadId) || manager.GetSquadById(selectedSquadId) == null)
                selectedSquadId = manager.squads.FirstOrDefault(s => s != null)?.squadId;
        }

        private void DrawPawnRow(Rect row, Pawn pawn, bool isLeader, Action onRemove)
        {
            Widgets.DrawHighlight(row);
            if (pawn == null)
            {
                Widgets.Label(row.ContractedBy(8f), "Missing pawn");
                return;
            }

            float iconSize = 40f;
            Rect portraitRect = new Rect(row.x + 4f, row.y + 4f, iconSize, iconSize);
            RenderTexture portrait = PortraitsCache.Get(
                pawn,
                new Vector2(iconSize, iconSize),
                Rot4.South,
                Vector3.zero,
                1f,
                supersample: true,
                compensateForUIScale: true,
                renderHeadgear: true,
                renderClothes: true,
                overrideApparelColors: null,
                overrideHairColor: null,
                stylingStation: false,
                healthStateOverride: null);
            GUI.DrawTexture(portraitRect, portrait);

            float textX = portraitRect.xMax + 6f;
            MilitaryStatComp comp = MilitaryUtility.GetComp(pawn);
            string role = isLeader ? "Leader" : "Member";
            string rank = comp?.rank ?? "Unranked";

            Rect nameRect = new Rect(textX, row.y + 4f, row.width - textX - 150f, 20f);
            Rect rankRect = new Rect(textX, row.y + 24f, row.width - textX - 150f, 20f);
            Widgets.Label(nameRect, $"{pawn.LabelShort} ({role})");
            Widgets.Label(rankRect, rank);

            if (comp != null && !string.IsNullOrEmpty(comp.rank))
            {
                Rect rankIconRect = new Rect(row.xMax - 102f, row.y + 12f, 24f, 24f);
                Widgets.DrawTextureFitted(rankIconRect, MilitaryUtility.GetRankTexture(comp.rank), 1f);
            }

            if (!isLeader && onRemove != null)
            {
                Rect removeRect = new Rect(row.xMax - 72f, row.y + 9f, 68f, 30f);
                if (Widgets.ButtonText(removeRect, "Remove"))
                    onRemove();
            }
        }
    }

    public class Dialog_CreateSquad : Window
    {
        private string squadName = "";
        private Pawn selectedLeader;
        private readonly Action<SquadData> onCreated;

        public override Vector2 InitialSize => new Vector2(520f, 220f);

        public Dialog_CreateSquad(Action<SquadData> onCreated)
        {
            this.onCreated = onCreated;
            doCloseX = true;
            closeOnClickedOutside = true;
            absorbInputAroundWindow = true;
            draggable = true;
            forcePause = true;
        }

        public override void DoWindowContents(Rect inRect)
        {
            GameComponent_MilitaryManager manager = GameComponent_MilitaryManager.Instance;
            if (manager == null)
            {
                Widgets.Label(inRect, "Military manager not found.");
                return;
            }

            float curY = 0f;

            Widgets.Label(new Rect(0f, curY, 120f, 28f), "Squad Name:");
            squadName = Widgets.TextField(new Rect(122f, curY, inRect.width - 122f, 28f), squadName ?? "");
            curY += 42f;

            Widgets.Label(new Rect(0f, curY, 120f, 28f), "Leader:");
            string leaderLabel = selectedLeader != null ? selectedLeader.LabelShort : "Select leader";
            if (Widgets.ButtonText(new Rect(122f, curY, 220f, 28f), leaderLabel))
            {
                List<FloatMenuOption> options = BuildLeaderOptions(manager);
                if (options.Count == 0)
                    options.Add(new FloatMenuOption("No eligible leaders", null));
                Find.WindowStack.Add(new FloatMenu(options));
            }
            curY += 56f;

            Rect confirmRect = new Rect(0f, curY, 120f, 32f);
            Rect cancelRect = new Rect(130f, curY, 120f, 32f);

            if (Widgets.ButtonText(confirmRect, "Confirm"))
            {
                SquadData squad = manager.CreateSquad(squadName, selectedLeader);
                if (squad != null)
                {
                    onCreated?.Invoke(squad);
                    Close();
                }
                else
                {
                    Messages.Message("Unable to create squad. Check leader and name.", MessageTypeDefOf.RejectInput, false);
                }
            }

            if (Widgets.ButtonText(cancelRect, "Cancel"))
                Close();
        }

        private List<FloatMenuOption> BuildLeaderOptions(GameComponent_MilitaryManager manager)
        {
            List<FloatMenuOption> options = new List<FloatMenuOption>();
            List<Pawn> pawns = PawnsFinder.AllMaps_FreeColonists.ToList();

            List<Pawn> candidates = new List<Pawn>();
            for (int i = 0; i < pawns.Count; i++)
            {
                Pawn pawn = pawns[i];
                if (!SquadData.IsValidLeader(pawn))
                    continue;
                if (!MilitaryUtility.IsEligible(pawn))
                    continue;
                if (manager.GetSquadOf(pawn) != null)
                    continue;

                candidates.Add(pawn);
            }

            bool hasLieutenant = false;
            for (int i = 0; i < candidates.Count; i++)
            {
                MilitaryStatComp lComp = MilitaryUtility.GetComp(candidates[i]);
                if (lComp != null && lComp.rank == "Lieutenant")
                {
                    hasLieutenant = true;
                    break;
                }
            }

            for (int i = 0; i < candidates.Count; i++)
            {
                Pawn candidate = candidates[i];
                MilitaryStatComp comp = MilitaryUtility.GetComp(candidate);
                if (hasLieutenant && (comp == null || comp.rank != "Lieutenant"))
                    continue;

                Pawn captured = candidate;
                options.Add(new FloatMenuOption(captured.LabelShort, () => selectedLeader = captured));
            }

            return options;
        }
    }
}
