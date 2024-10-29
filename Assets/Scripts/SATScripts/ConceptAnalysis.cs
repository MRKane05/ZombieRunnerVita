using UnityEngine;
using System.Collections.Generic;

public class ConceptAnalysis : MonoBehaviour
{

    public Animator animator; // Assign the Animator in the Inspector
    public HumanBodyBones targetBone; // Choose which bone to sample (e.g., Hips)

    void Start()
    {
        // Get the Animator component
        Animator animator = GetComponent<Animator>();
        Debug.Log(gameObject.name);
        //ListAllbones();
        SampleBoneAtTime(1f);
    }

    void ListAllBones()
    {
        if (animator != null && animator.avatar.isHuman)
        {
            // Create a list to store bone transforms
            List<Transform> boneTransforms = new List<Transform>();

            // Iterate over all the HumanBodyBones and fetch their transforms
            foreach (HumanBodyBones bone in System.Enum.GetValues(typeof(HumanBodyBones)))
            {
                // Ignore invalid bones
                if (bone == HumanBodyBones.LastBone) continue;

                Transform boneTransform = animator.GetBoneTransform(bone);

                if (boneTransform != null)
                {
                    boneTransforms.Add(boneTransform);
                    Debug.Log("Bone: " + bone + " Transform: " + boneTransform.name + " bone index: " + (int)bone);
                }
            }

            // Output the count of bones found
            Debug.Log("Total bones found: " + boneTransforms.Count);
        }
        else
        {
            Debug.LogError("No Animator or Avatar is not humanoid!");
        }

    }

    void SampleBoneAtTime(float normalizedTime)
    {
        // Make sure the animator is in the correct state
        AnimatorStateInfo animStateInfo = animator.GetCurrentAnimatorStateInfo(0);

        // Set the normalized time of the animation
        animator.Play(animStateInfo.fullPathHash, 0, normalizedTime);

        // Update the animator to apply the new time
        animator.Update(0);

        // Retrieve the target bone's transform
        Transform boneTransform = animator.GetBoneTransform(targetBone);

        if (boneTransform != null)
        {
            // Get the position and rotation after the animation is applied
            Vector3 animatedPosition = boneTransform.position;
            Quaternion animatedRotation = boneTransform.rotation;

            // Get the position in the local space of the model
            Vector3 localPosition = boneTransform.localPosition;
            Quaternion localRotation = boneTransform.localRotation;

            // You can now calculate an offset from the bone's pose position
            Vector3 posePosition = animator.GetBoneTransform(targetBone).position; // Pose position (T-pose or rest position)
            Quaternion poseRotation = animator.GetBoneTransform(targetBone).rotation;

            // Offset relative to the rest pose
            Vector3 positionOffset = animatedPosition - posePosition;
            Quaternion rotationOffset = Quaternion.Inverse(poseRotation) * animatedRotation;

            // Optionally, log the data
            Debug.Log("Bone: " + targetBone + " Position: " + animatedPosition + " Rotation: " + animatedRotation);
            Debug.Log("Position Offset: " + positionOffset + " Rotation Offset: " + rotationOffset.eulerAngles);
        }
        else
        {
            Debug.LogWarning("Bone " + targetBone + " not found.");
        }
    }
}