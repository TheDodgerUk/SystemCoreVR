
using UnityEngine;
using Autohand;
using TMPro;
using System.Collections;

[RequireComponent(typeof(Grabbable))]
[RequireComponent(typeof(Rigidbody))]
public class PalmCard : MonoBehaviour
{
    [Header("Card Data")]
    public CardData cardData;

    [Header("Card Display")]
    public TextMeshPro titleText;
    public TextMeshPro descriptionText;
    public SpriteRenderer cardImage;

    private Grabbable grab;
    private Rigidbody rb;
    private PalmCardHolder holder;

    private Transform originalParent;
    private Vector3 originalLocalPosition;
    private Quaternion originalLocalRotation;

    private bool selected;

    public bool IsSelected => selected;

    private void Awake()
    {
        grab = GetComponent<Grabbable>();
        rb = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        grab.OnGrabEvent += OnGrab;
        grab.OnReleaseEvent += OnRelease;
    }

    private void OnDisable()
    {
        grab.OnGrabEvent -= OnGrab;
        grab.OnReleaseEvent -= OnRelease;
    }

    public void Initialise(PalmCardHolder owner)
    {
        holder = owner;

        originalParent = transform.parent;
        originalLocalPosition = transform.localPosition;
        originalLocalRotation = transform.localRotation;
    }

    public void UpdateDisplay()
    {
        if (cardData == null)
            return;

        if (titleText != null)
            titleText.text = cardData.cardName;

        if (descriptionText != null)
            descriptionText.text = cardData.description;

        if (cardImage != null)
            cardImage.sprite = cardData.cardImage;
    }

    private void OnGrab(Hand hand, Grabbable grabbable)
    {
        if (selected)
            return;

        if (holder == null ||
            !holder.IsSelectionHand(hand))
            return;

        selected = true;

        // Detach from the palm.
        transform.SetParent(null, true);

        rb.isKinematic = false;
        rb.useGravity = false;

        holder.CardSelected(this, hand);
    }

    private void OnRelease(Hand hand, Grabbable grabbable)
    {
        if (!selected)
            return;

        StartCoroutine(ReturnToPalm());
    }

    private IEnumerator ReturnToPalm()
    {
        // Wait until AutoHand has released the card.
        yield return new WaitForEndOfFrame();

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.isKinematic = true;
        rb.useGravity = false;

        // Restore its original position in the palm.
        transform.SetParent(originalParent, false);
        transform.localPosition = originalLocalPosition;
        transform.localRotation = originalLocalRotation;

        selected = false;

        if (holder != null)
            holder.CardReturned(this);
    }
}
