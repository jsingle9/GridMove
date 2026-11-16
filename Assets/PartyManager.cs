using System;
using System.Collections.Generic;
using UnityEngine;

public class PartyManager : MonoBehaviour
{
    public static PartyManager Instance { get; private set; }

    [SerializeField] private List<BoxMover> partyMembers = new List<BoxMover>();
    private int activeIndex = 0;

    public event Action<BoxMover> OnPartyMemberChanged;
    public event Action<List<BoxMover>> OnPartyUpdated;

    public BoxMover ActiveMember => activeIndex >= 0 && activeIndex < partyMembers.Count
        ? partyMembers[activeIndex]
        : null;

    public List<BoxMover> Members => partyMembers;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetPartyMembers(List<BoxMover> members)
    {
        partyMembers = members;
        activeIndex = 0;
        OnPartyUpdated?.Invoke(partyMembers);
        OnPartyMemberChanged?.Invoke(ActiveMember);
    }

    public void AddMember(BoxMover member)
    {
        if (!partyMembers.Contains(member))
        {
            partyMembers.Add(member);
            OnPartyUpdated?.Invoke(partyMembers);
        }
    }

    public void RemoveMember(BoxMover member)
    {
        if (partyMembers.Contains(member))
        {
            partyMembers.Remove(member);
            if (ActiveMember == member)
            {
                activeIndex = 0;
            }
            OnPartyUpdated?.Invoke(partyMembers);
            OnPartyMemberChanged?.Invoke(ActiveMember);
        }
    }

    public void SetActive(BoxMover member)
    {
        int index = partyMembers.IndexOf(member);
        if (index >= 0 && index != activeIndex)
        {
            activeIndex = index;
            OnPartyMemberChanged?.Invoke(ActiveMember);
        }
    }

    public void SetActiveByIndex(int index)
    {
        if (index >= 0 && index < partyMembers.Count && index != activeIndex)
        {
            activeIndex = index;
            OnPartyMemberChanged?.Invoke(ActiveMember);
        }
    }

    public int GetActiveIndex()
    {
        return activeIndex;
    }

    public bool IsPlayerControlled(ICombatant combatant)
    {
        return partyMembers.Contains(combatant as BoxMover);
    }

    public List<ICombatant> GetPartyAsCombatants()
    {
        List<ICombatant> result = new List<ICombatant>();
        foreach (var member in partyMembers)
        {
            if (member != null)
                result.Add(member as ICombatant);
        }
        return result;
    }
}
