﻿using DarkUI.Forms;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using TombLib.Graphics;
using TombLib.Utils;
using TombLib.Wad;

namespace WadTool
{
    public class AnimationEditor
    {
        public List<AnimationNode> WorkingAnimations;
        public AnimationNode SelectedNode;

        public WadToolClass Tool { get; private set; }
        public Wad2 Wad { get; private set; }
        public WadMoveable Moveable { get; private set; }

        // Clipboard
        public List<KeyFrame> ClipboardKeyFrames = new List<KeyFrame>();
        public AnimationNode ClipboardNode = null;

        // General state
        public bool Saved;

        public AnimationEditor(WadToolClass tool, Wad2 wad, WadMoveableId idToEdit)
        {
            Tool = tool;
            Wad = wad;
            Moveable = Wad.Moveables.ContainsKey(idToEdit) ? Wad.Moveables[idToEdit] : null;

            // NOTE: we work with a pair WadAnimation - Animation. All changes to animation data like name,
            // framerate, next animation, state changes... will be saved directly to WadAnimation.
            // All changes to keyframes will be instead stored directly in the renderer's Animation class.
            // While saving, WadAnimation and Animation will be combined and original animations will be overwritten.

            WorkingAnimations = new List<AnimationNode>();
            for (int i = 0; i < Moveable.Animations.Count; i++)
            {
                var animation = Moveable.Animations[i];
                WorkingAnimations.Add(new AnimationNode(animation.Clone(), Animation.FromWad2(Moveable.Bones, animation), i));
            }
        }

        public bool ValidAnimationAndFrames => SelectedNode != null && SelectedNode.DirectXAnimation.KeyFrames.Count > 0;

        public WadAnimation GetSavedAnimation(AnimationNode animation)
        {
            var wadAnim = animation.WadAnimation.Clone();
            var directxAnim = animation.DirectXAnimation;

            // I need only to convert DX keyframes to Wad2 keyframes
            wadAnim.KeyFrames.Clear();
            foreach (var directxKeyframe in directxAnim.KeyFrames)
            {
                var keyframe = new WadKeyFrame();

                // Create the new bounding box
                keyframe.BoundingBox = new TombLib.BoundingBox(directxKeyframe.BoundingBox.Minimum,
                                                               directxKeyframe.BoundingBox.Maximum);

                // For now we take the first translation as offset
                keyframe.Offset = new System.Numerics.Vector3(directxKeyframe.Translations[0].X,
                                                              directxKeyframe.Translations[0].Y,
                                                              directxKeyframe.Translations[0].Z);

                // Convert angles from radians to degrees and save them
                foreach (var rot in directxKeyframe.Rotations)
                {
                    var angle = new WadKeyFrameRotation();
                    angle.Rotations = new System.Numerics.Vector3(rot.X * 180.0f / (float)Math.PI,
                                                                  rot.Y * 180.0f / (float)Math.PI,
                                                                  rot.Z * 180.0f / (float)Math.PI);
                    keyframe.Angles.Add(angle);
                }

                wadAnim.KeyFrames.Add(keyframe);
            }

            return wadAnim;
        }

        public bool SaveChanges(IWin32Window owner = null)  // No owner - no confirmation!
        {
            if (owner != null && DarkMessageBox.Show(owner, "Do you really want to save changes to animations?", "Save changes",
                                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return false;

            // Clear the old animations
            Moveable.Animations.Clear();

            // Combine WadAnimation and Animation classes
            foreach (var animation in WorkingAnimations)
                Moveable.Animations.Add(GetSavedAnimation(animation));

            Moveable.Version = DataVersion.GetNext();
            return true;
        }
    }
}