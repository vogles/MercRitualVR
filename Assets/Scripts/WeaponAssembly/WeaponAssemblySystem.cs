﻿using Messaging;
using UnityEngine;

/// <summary>
/// This should really be a singleton but...oh well
/// </summary>
public class WeaponAssemblySystem : MonoBehaviour
{
    /// <summary>
    /// You have two hands
    /// You can grab up to two things
    /// If both things collide and have a <see cref="WeaponPiece"/>, then snap them together
    /// </summary>
    /// <remarks>
    /// Left Hand - 0
    /// Right Hand - 1
    /// </remarks>
    private Object[] _currentlyGrabbedItems = new Object[2];

    [SerializeField]
    private GameObject _triggerComboPrefab = null;

    [SerializeField]
    private GameObject _bodyComboPrefab = null;

    [SerializeField]
    private GameObject _fullGunPrefab = null;

    void OnEnable()
    {
        MessageSystem.Default.Subscribe<PickupItemMessage>(OnPickedUpItem);
        MessageSystem.Default.Subscribe<DropItemMessage>(OnDroppedItem);
        MessageSystem.Default.Subscribe<TrySnapItemsMessage>(OnTryToSnapItems);
        MessageSystem.Default.Subscribe<AcquiredWeaponComboMessage>(OnNewCombo);
        MessageSystem.Default.Subscribe<AcquiredWeaponMessage>(OnNewGun);
    }

    void OnDisable()
    {
        MessageSystem.Default.Unsubscribe<PickupItemMessage>(OnPickedUpItem);
        MessageSystem.Default.Unsubscribe<DropItemMessage>(OnDroppedItem);
        MessageSystem.Default.Unsubscribe<TrySnapItemsMessage>(OnTryToSnapItems);
        MessageSystem.Default.Unsubscribe<AcquiredWeaponComboMessage>(OnNewCombo);
        MessageSystem.Default.Unsubscribe<AcquiredWeaponMessage>(OnNewGun);
    }

    private void OnNewGun(AcquiredWeaponMessage obj)
    {
        Destroy((_currentlyGrabbedItems[0] as WeaponPiece).gameObject);
        Destroy((_currentlyGrabbedItems[1] as WeaponPiece).gameObject);

        _currentlyGrabbedItems[0] = null;
        _currentlyGrabbedItems[1] = null;
    }

    private void OnPickedUpItem(PickupItemMessage msg)
    {
        Debug.Log("Grabbing " + msg.ItemPickedUp.name + " with hand " + msg.Hand);
        _currentlyGrabbedItems[msg.Hand] = msg.ItemPickedUp.GetComponent<WeaponPiece>();
    }

    private void OnNewCombo(AcquiredWeaponComboMessage obj)
    {
        if (obj.ItemCreated != null)
        {
            var newTransform = obj.ItemCreated.transform;
            newTransform.parent = transform;
            newTransform.localPosition = Vector3.up;
        }

        OnNewGun(null);
    }

    private void OnDroppedItem(DropItemMessage msg)
    {
        if (_currentlyGrabbedItems[msg.Hand] != null)
            Debug.Log("Dropping " + _currentlyGrabbedItems[msg.Hand].name);
        _currentlyGrabbedItems[msg.Hand] = null;
    }

    private void OnTryToSnapItems(TrySnapItemsMessage msg)
    {
        if (_currentlyGrabbedItems[0] != null && _currentlyGrabbedItems[1] != null)
        {
            var hand0 = _currentlyGrabbedItems[0] as WeaponPiece;
            var hand1 = _currentlyGrabbedItems[1] as WeaponPiece;

            GameObject newCombo = null;

            // Knife and Spoon
            if (hand0.Type == WeaponPiece.WeaponPieceType.Trigger && hand1.Type == WeaponPiece.WeaponPieceType.Trigger && _triggerComboPrefab != null)
            {
                newCombo = Instantiate(_triggerComboPrefab);
                MessageSystem.Default.Broadcast(new AcquiredWeaponComboMessage { ItemCreated = newCombo });
            }
            else
            {
                Debug.Log("Trying to combine: " + hand0.name + " and " + hand1.name);
                var haveNozzle = hand0.Type == WeaponPiece.WeaponPieceType.Nozzle || hand1.Type == WeaponPiece.WeaponPieceType.Nozzle;
                var haveResonator = hand0.Type == WeaponPiece.WeaponPieceType.Resonator || hand1.Type == WeaponPiece.WeaponPieceType.Resonator;
                var haveBodyCombo = hand0.Type == WeaponPiece.WeaponPieceType.CraftedBody || hand1.Type == WeaponPiece.WeaponPieceType.CraftedBody;
                var haveTriggerCombo = hand0.Type == WeaponPiece.WeaponPieceType.CraftedTrigger || hand1.Type == WeaponPiece.WeaponPieceType.CraftedTrigger;

                // Bowl and Tin Can
                if (haveNozzle && haveResonator && _bodyComboPrefab != null)
                {
                    newCombo = Instantiate(_bodyComboPrefab);
                    MessageSystem.Default.Broadcast(new AcquiredWeaponComboMessage { ItemCreated = newCombo });
                }

                if (haveBodyCombo && haveTriggerCombo && _fullGunPrefab != null)
                {
                    _fullGunPrefab.SetActive(true);
                    MessageSystem.Default.Broadcast(new AcquiredWeaponMessage());
                }
            }
        }
    }
}
