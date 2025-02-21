INCLUDE Global.ink

VAR SPEAKER_ID = ""
VAR SPEAKER_IDS = "Fairy_Id"
{ SPEAKER_ID:
    - "" : -> Default
    - "Fairy_Id" : -> FairyId
}

VAR QUEST_ID = ""
VAR REWARD_ID = ""

~ SPEAKER_IDS = "Fairy_Id"

=== Default ===
บ่อนํ้านี้ดูสะอาดดีนะ

+ [ดื่มนํ้า]
{   - not RequirementCheck("QuestRequirement", "curselake_quest_id") && not RequirementCheck("CurrentQuestRequirement", "curselake_quest_id"):
        -> StartQuest
        
    - else:
        -> DrinkAndDie
}
+ [ไม่ทำอะไร]
    -> END

=== StartQuest ===
#from :???
หยุดก่อนอย่าดื่มนํ้านั่นนะ
~ AddQuest("curselake_quest_id")
-> END

=== DrinkAndDie ===
#from :Player
ดูเหมือนว่าน้ำในบ่อนี้จะมีอะไรแปลกนะ...
#from :Player
อัก..เอื้อ..
~ KillPlayer()
-> END

=== FairyId ===
Yo
-> END
