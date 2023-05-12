using HTC.UnityPlugin.Utility;
using HTC.UnityPlugin.Vive;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.HTC.Gesture
{
    public static class GestureRecognizeMethod 
    {
        private static ViveRoleProperty leftHandRole = ViveRoleProperty.New(TrackedHandRole.LeftHand);
        private static ViveRoleProperty rightHandRole = ViveRoleProperty.New(TrackedHandRole.RightHand);
        private static ViveRoleProperty headRole = ViveRoleProperty.New(DeviceRole.Hmd);

        public struct FingerCurvatureInfo
        {
            public float min;
            public float max;
        }

        public static readonly FingerCurvatureInfo IgnoreCurvature = new FingerCurvatureInfo
        {
            min = 0,
            max = 180
        };

        public static bool CheckHandOpen(ViveRoleProperty role)
        {
            return CheckFingerCurvatureSatisfied(role,
                IgnoreCurvature,
                new FingerCurvatureInfo() { min = 0, max = 45 },
                new FingerCurvatureInfo() { min = 0, max = 45 },
                new FingerCurvatureInfo() { min = 0, max = 45 },
                new FingerCurvatureInfo() { min = 0, max = 45 }
                );
        }

        public static bool CheckHandHold(ViveRoleProperty role, float validRaio = 1)
        {
            return CheckFingerCurvatureSatisfied(role,
                IgnoreCurvature,
                new FingerCurvatureInfo() { min = 100, max = 180 },
                new FingerCurvatureInfo() { min = 100, max = 180 },
                new FingerCurvatureInfo() { min = 100, max = 180 },
                new FingerCurvatureInfo() { min = 100, max = 180 },
                validRaio);
        }

        public static bool CheckFingerPurse(ViveRoleProperty role, float validRaio = 0.8f)
        {
            return CheckFingerDistanceSatisfied(role,
                0.025f,
                0.025f,
                0.025f,
                0.025f,
                validRaio);
        }

        public static bool CheckFingerCurvatureSatisfied(ViveRoleProperty role, FingerCurvatureInfo thumbInfo, FingerCurvatureInfo indexInfo, FingerCurvatureInfo middleInfo, FingerCurvatureInfo ringInfo, FingerCurvatureInfo pinkyInfo, float validRatio = 1f)
        {
            JointPose[] joints = new JointPose[20];

            VivePose.TryGetHandJointPose(role, HandJointName.ThumbMetacarpal, out joints[0]);
            VivePose.TryGetHandJointPose(role, HandJointName.ThumbProximal, out joints[1]);
            VivePose.TryGetHandJointPose(role, HandJointName.ThumbDistal, out joints[2]);
            VivePose.TryGetHandJointPose(role, HandJointName.ThumbTip, out joints[3]);

            VivePose.TryGetHandJointPose(role, HandJointName.IndexProximal, out joints[4]);
            VivePose.TryGetHandJointPose(role, HandJointName.IndexIntermediate, out joints[5]);
            VivePose.TryGetHandJointPose(role, HandJointName.IndexDistal, out joints[6]);
            VivePose.TryGetHandJointPose(role, HandJointName.IndexTip, out joints[7]);

            VivePose.TryGetHandJointPose(role, HandJointName.MiddleProximal, out joints[8]);
            VivePose.TryGetHandJointPose(role, HandJointName.MiddleIntermediate, out joints[9]);
            VivePose.TryGetHandJointPose(role, HandJointName.MiddleDistal, out joints[10]);
            VivePose.TryGetHandJointPose(role, HandJointName.MiddleTip, out joints[11]);

            VivePose.TryGetHandJointPose(role, HandJointName.RingProximal, out joints[12]);
            VivePose.TryGetHandJointPose(role, HandJointName.RingIntermediate, out joints[13]);
            VivePose.TryGetHandJointPose(role, HandJointName.RingDistal, out joints[14]);
            VivePose.TryGetHandJointPose(role, HandJointName.RingTip, out joints[15]);

            VivePose.TryGetHandJointPose(role, HandJointName.PinkyProximal, out joints[16]);
            VivePose.TryGetHandJointPose(role, HandJointName.PinkyIntermediate, out joints[17]);
            VivePose.TryGetHandJointPose(role, HandJointName.PinkyDistal, out joints[18]);
            VivePose.TryGetHandJointPose(role, HandJointName.PinkyTip, out joints[19]);

            float curValidRatio = 0;
            if (checkAngleSatisfied(jointVec(joints, 1, 2), jointVec(joints, 2, 3), thumbInfo)) curValidRatio += 0.2f;
            if (checkAngleSatisfied(jointVec(joints, 4, 5), jointVec(joints, 6, 7), indexInfo)) curValidRatio += 0.2f;
            if (checkAngleSatisfied(jointVec(joints, 8, 9), jointVec(joints, 10, 11), middleInfo)) curValidRatio += 0.2f;
            if (checkAngleSatisfied(jointVec(joints, 12, 13), jointVec(joints, 14, 15), ringInfo)) curValidRatio += 0.2f;
            if (checkAngleSatisfied(jointVec(joints, 16, 17), jointVec(joints, 18, 19), pinkyInfo)) curValidRatio += 0.2f;

            return curValidRatio >= validRatio;
        }

        public static bool CheckFingerDistanceSatisfied(ViveRoleProperty role, float thumbThreshold, float indexThreshold, float middleThreshold, float ringThreshold, float validRatio = 0.8f)
        {
            JointPose[] joints = new JointPose[20];

            VivePose.TryGetHandJointPose(role, HandJointName.ThumbMetacarpal, out joints[0]);
            VivePose.TryGetHandJointPose(role, HandJointName.ThumbProximal, out joints[1]);
            VivePose.TryGetHandJointPose(role, HandJointName.ThumbDistal, out joints[2]);
            VivePose.TryGetHandJointPose(role, HandJointName.ThumbTip, out joints[3]);

            VivePose.TryGetHandJointPose(role, HandJointName.IndexProximal, out joints[4]);
            VivePose.TryGetHandJointPose(role, HandJointName.IndexIntermediate, out joints[5]);
            VivePose.TryGetHandJointPose(role, HandJointName.IndexDistal, out joints[6]);
            VivePose.TryGetHandJointPose(role, HandJointName.IndexTip, out joints[7]);

            VivePose.TryGetHandJointPose(role, HandJointName.MiddleProximal, out joints[8]);
            VivePose.TryGetHandJointPose(role, HandJointName.MiddleIntermediate, out joints[9]);
            VivePose.TryGetHandJointPose(role, HandJointName.MiddleDistal, out joints[10]);
            VivePose.TryGetHandJointPose(role, HandJointName.MiddleTip, out joints[11]);

            VivePose.TryGetHandJointPose(role, HandJointName.RingProximal, out joints[12]);
            VivePose.TryGetHandJointPose(role, HandJointName.RingIntermediate, out joints[13]);
            VivePose.TryGetHandJointPose(role, HandJointName.RingDistal, out joints[14]);
            VivePose.TryGetHandJointPose(role, HandJointName.RingTip, out joints[15]);

            VivePose.TryGetHandJointPose(role, HandJointName.PinkyProximal, out joints[16]);
            VivePose.TryGetHandJointPose(role, HandJointName.PinkyIntermediate, out joints[17]);
            VivePose.TryGetHandJointPose(role, HandJointName.PinkyDistal, out joints[18]);
            VivePose.TryGetHandJointPose(role, HandJointName.PinkyTip, out joints[19]);

            float curValidRatio = 0;
            if (Vector3.Distance(joints[3].pose.pos, joints[7].pose.pos) <= thumbThreshold
                || Vector3.Distance(joints[3].pose.pos, joints[6].pose.pos) <= thumbThreshold) curValidRatio += 0.2f;
            if (Vector3.Distance(joints[7].pose.pos, joints[11].pose.pos) <= indexThreshold) curValidRatio += 0.2f;
            if (Vector3.Distance(joints[11].pose.pos, joints[15].pose.pos) <= middleThreshold) curValidRatio += 0.2f;
            if (Vector3.Distance(joints[15].pose.pos, joints[19].pose.pos) <= ringThreshold) curValidRatio += 0.2f;

            return curValidRatio >= validRatio;
        }

        private static Vector3 jointVec(JointPose[] joints, int v1, int v2)
        {
            return joints[v2].pose.pos - joints[v1].pose.pos;
        }

        private static bool checkAngleSatisfied(Vector3 v1, Vector3 v2, FingerCurvatureInfo curvatureInfo)
        {
            float angle = Vector3.Angle(v1, v2);
            return angle >= curvatureInfo.min && angle < curvatureInfo.max;
        }
    
        public static bool CheckHandFacingVector(ViveRoleProperty role, Vector3 vec, float tolerateAngle)
        {
            JointPose wristPose;
            
            if (!VivePose.TryGetHandJointPose(role, HandJointName.Wrist, out wristPose)) return false;
            
            float palmNormal_AngleFromVec = Vector3.Angle(vec, wristPose.pose.rot * Vector3.down);
            
            return palmNormal_AngleFromVec < tolerateAngle;
        }

        public static bool CheckHMDFacingHand(ViveRoleProperty role, float tolerateAngle)
        {
            JointPose wristPose;

            if (!VivePose.TryGetHandJointPose(role, HandJointName.Wrist, out wristPose)) return false;
            
            RigidPose hmdPose = VivePose.GetPose(DeviceRole.Hmd);
            
            //Facing hand horizontally

            Vector3 dirHmdToHandRole = wristPose.pose.pos - hmdPose.pos;
            dirHmdToHandRole.Scale(new Vector3(1, 0, 1));

            Vector3 dirHmdForward = hmdPose.forward;
            dirHmdForward.Scale(new Vector3(1, 0, 1));

            float angleFromPalmToFoward = Vector3.Angle(dirHmdForward, dirHmdToHandRole);
            
            return angleFromPalmToFoward < tolerateAngle;
        }

        public static bool CheckPalmFacingHMD(ViveRoleProperty handRole, float tolerateAngle)
        {
            JointPose palmPose;

            if (!VivePose.TryGetHandJointPose(handRole, HandJointName.Palm, out palmPose)) return false;

            RigidPose hmdPose = VivePose.GetPose(DeviceRole.Hmd);

            Vector3 dirHandToHmd = hmdPose.pos - palmPose.pose.pos;

            float angleFromPalmToHMD = Vector3.Angle(dirHandToHmd, -palmPose.pose.up);

            return angleFromPalmToHMD < tolerateAngle;
        }
    }
}