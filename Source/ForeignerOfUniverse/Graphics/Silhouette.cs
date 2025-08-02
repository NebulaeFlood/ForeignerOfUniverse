using RimWorld;
using UnityEngine;
using Verse;

namespace ForeignerOfUniverse.Graphics
{
    public sealed class Silhouette : Graphic_Mote
    {
        protected override bool ForcePropertyBlock => true;


        public override void Init(GraphicRequest req)
        {
            data = req.graphicData;
            path = req.path;
            maskPath = req.maskPath;
            color = req.color;
            colorTwo = req.colorTwo;
            drawSize = req.drawSize;

            _request = req;
        }

        public override void DrawWorker(Vector3 loc, Rot4 rot, ThingDef thingDef, Thing thing, float extraRotation)
        {
            Mote mote = (Mote)thing;
            Thing targetThing = mote.link1.Target.Thing;

            if (targetThing is null)
            {
                return;
            }

            if (TryGetPawn(targetThing, out var pawn))
            {
                DrawPawn(pawn, mote);
            }
        }


        //------------------------------------------------------
        //
        //  Private Static Methods
        //
        //------------------------------------------------------

        #region Private Static Methods

        private static Vector3 CalculatePawnDrawPosition(Pawn pawn, out Building_Bed bed)
        {
            if (pawn.Dead)
            {
                bed = null;
                return pawn.Corpse.DrawPos;
            }

            bed = pawn.CurrentBed();
            var drawPos = pawn.DrawPos;

            if (bed != null)
            {
                Rot4 rotation = bed.Rotation;
                rotation.AsInt += 2;
                drawPos -= rotation.FacingCell.ToVector3() * (pawn.story.bodyType.bedOffset + pawn.Drawer.renderer.BaseHeadOffsetAt(Rot4.South).z);
            }

            return drawPos;
        }

        private static Material MakeMaterialFromRequest(GraphicRequest req, Texture mainTex)
        {
            return MaterialPool.MatFrom(new MaterialRequest
            {
                mainTex = mainTex,
                shader = req.shader,
                color = req.color,
                colorTwo = req.colorTwo,
                renderQueue = req.renderQueue,
                shaderParameters = req.shaderParameters
            });
        }

        private static bool TryGetPawn(Thing thing, out Pawn pawn)
        {
            pawn = thing as Pawn ?? (thing as Corpse)?.InnerPawn;
            return pawn != null;
        }

        #endregion


        private void DrawPawn(Pawn pawn, Mote mote)
        {
            var renderer = pawn.Drawer?.renderer;

            if (renderer is null || !renderer.renderTree.Resolved)
            {
                return;
            }

            var color = this.color;
            color.a *= mote.Alpha;

            bool isHumanlike = pawn.RaceProps.Humanlike;
            bool isStanding = pawn.GetPosture() is PawnPosture.Standing;
            Rot4 pawnFacing = isStanding ? pawn.Rotation : renderer.LayingFacing();

            Vector3 drawPos = CalculatePawnDrawPosition(pawn, out var bed);
            drawPos.y = mote.def.Altitude;

            if (!ReferenceEquals(_lastPawn, pawn) || _lastFacing != pawnFacing)
            {
                _bodyMaterial = MakeMaterialFromRequest(_request, renderer.BodyGraphic.MatAt(pawnFacing).mainTexture);

                if (isHumanlike)
                {
                    _headMaterial = renderer.HeadGraphic is null ? null : MakeMaterialFromRequest(_request, renderer.HeadGraphic.MatAt(pawnFacing).mainTexture);
                }
            }

            Mesh bodyMesh = isHumanlike
                ? HumanlikeMeshPoolUtility.GetHumanlikeBodySetForPawn(pawn).MeshAt(pawnFacing)
                : renderer.BodyGraphic.MeshAt(pawnFacing);

            _bodyMaterial.SetVector(ShaderPropertyIDs.PawnCenterWorld, new Vector4(drawPos.x, drawPos.z, 0f, 0f));
            _bodyMaterial.SetVector(ShaderPropertyIDs.PawnDrawSizeWorld, new Vector4(bodyMesh.bounds.size.x, bodyMesh.bounds.size.z, 0f, 0f));
            _bodyMaterial.SetFloat(ShaderPropertyIDs.AgeSecs, mote.AgeSecs);
            _bodyMaterial.SetColor(ShaderPropertyIDs.Color, color);

            var bodyRotation = Quaternion.AngleAxis(isStanding ? 0f : renderer.BodyAngle(PawnRenderFlags.None), Vector3.up);

            if (bed is null || bed.def.building.bed_showSleeperBody)
            {
                GenDraw.DrawMeshNowOrLater(bodyMesh, drawPos, bodyRotation, _bodyMaterial, false);
            }

            if (isHumanlike && _headMaterial != null)
            {
                Vector3 headOffset = bodyRotation * renderer.BaseHeadOffsetAt(pawnFacing) + new Vector3(0f, 0.001f, 0f);
                Mesh headMesh = HumanlikeMeshPoolUtility.GetHumanlikeHeadSetForPawn(pawn).MeshAt(pawnFacing);

                _headMaterial.SetVector(ShaderPropertyIDs.PawnCenterWorld, new Vector4(drawPos.x, drawPos.z, 0f, 0f));
                _headMaterial.SetVector(ShaderPropertyIDs.PawnDrawSizeWorld, new Vector4(headMesh.bounds.size.x, bodyMesh.bounds.size.z, 0f, 0f));
                _headMaterial.SetFloat(ShaderPropertyIDs.AgeSecs, mote.AgeSecs);
                _headMaterial.SetColor(ShaderPropertyIDs.Color, color);

                GenDraw.DrawMeshNowOrLater(headMesh, drawPos + headOffset, bodyRotation, _headMaterial, false);
            }

            _lastFacing = pawnFacing;
            _lastPawn = pawn;
        }


        //------------------------------------------------------
        //
        //  Private Fields
        //
        //------------------------------------------------------

        #region Private Fields

        private GraphicRequest _request;

        private Pawn _lastPawn;
        private Rot4 _lastFacing;
        private Material _bodyMaterial;
        private Material _headMaterial;

        #endregion
    }
}
