using UnityEngine;
using System.Collections;

public class VRDoubleDoor : MonoBehaviour
{
    public Transform leftPivot;
    public Transform rightPivot;

    public Vector3 leftHingeAxis = Vector3.up;
    public Vector3 rightHingeAxis = Vector3.up;

    public float openAngle = 90f;
    public float openSpeed = 2f;

    [Header("Sound Effects")]
    [SerializeField] private AudioClip doorOpenSound;
    [SerializeField] private AudioClip doorCloseSound;
    [SerializeField] private float closeSoundDelay = 0.7f;
    private AudioSource audioSource;

    private bool isOpen = false;
    private Quaternion leftClosed;
    private Quaternion rightClosed;
    private Quaternion leftOpen;
    private Quaternion rightOpen;

    void Start()
    {
        leftClosed = leftPivot.localRotation;
        rightClosed = rightPivot.localRotation;

        leftOpen = Quaternion.AngleAxis(openAngle, leftHingeAxis) * leftClosed;
        rightOpen = Quaternion.AngleAxis(-openAngle, rightHingeAxis) * rightClosed;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f;
    }

    public void ToggleDoor()
    {
        isOpen = !isOpen;
        StopAllCoroutines();
        StartCoroutine(AnimateDoors(isOpen));
    }

    IEnumerator AnimateDoors(bool open)
    {
        Quaternion lTarget = open ? leftOpen : leftClosed;
        Quaternion rTarget = open ? rightOpen : rightClosed;

        if (open)
        {
            if (doorOpenSound != null)
            {
                audioSource.PlayOneShot(doorOpenSound);
            }
        }
        else
        {
            // Start a coroutine to play close sound after delay
            StartCoroutine(PlayCloseSoundWithDelay());
        }

        while (
            Quaternion.Angle(leftPivot.localRotation, lTarget) > 0.1f ||
            Quaternion.Angle(rightPivot.localRotation, rTarget) > 0.1f
        )
        {
            leftPivot.localRotation =
                Quaternion.Slerp(leftPivot.localRotation, lTarget, Time.deltaTime * openSpeed);
            rightPivot.localRotation =
                Quaternion.Slerp(rightPivot.localRotation, rTarget, Time.deltaTime * openSpeed);

            yield return null;
        }

        leftPivot.localRotation = lTarget;
        rightPivot.localRotation = rTarget;
    }

    IEnumerator PlayCloseSoundWithDelay()
    {
        yield return new WaitForSeconds(closeSoundDelay);

        if (doorCloseSound != null)
        {
            audioSource.PlayOneShot(doorCloseSound);
        }
    }

    public bool IsAnimating()
    {
        Quaternion lTarget = isOpen ? leftOpen : leftClosed;
        Quaternion rTarget = isOpen ? rightOpen : rightClosed;

        return Quaternion.Angle(leftPivot.localRotation, lTarget) > 0.01f ||
               Quaternion.Angle(rightPivot.localRotation, rTarget) > 0.01f;
    }
}