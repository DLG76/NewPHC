INCLUDE Global.ink

VAR SPEAKER_ID = ""
VAR SPEAKER_IDS = ""
{ SPEAKER_ID:
    - "" : -> Default
}

VAR QUEST_ID = ""
VAR REWARD_ID = ""

=== Default ===
#from :Player
เหมือนวานี่จะเคยเป๊นที่อยู่อาศัยนะ...
+ [คุ้ยหาของเผื่อจะมีอะไรดีๆ]
{   - not RequirementCheck("QuestRequirement", "ancientchest_quest_id") && not RequirementCheck("CurrentQuestRequirement", "ancientchest_quest_id"):
        -> StartQuest
        
    - else:
        -> GetNothing
}
+ [ไม่ทำอะไร]
    -> END

=== StartQuest ===
#from :Player
เหมือนจะเจออะไรจริงๆแฮะ
#from :Player
อะไรว้าา..อุตส่าห์เจอกล่องแต่เปิดไม่ได้หรอ ไร้ประโยชน์เหมือนใตรเลยนะ
+ [ฺฮิว]
    ~ AddQuest("ancientchest_quest_id")
    ~ AddItem("ancientchest_id")
    -> END
+ [ฺไอฮิว]
    ~ AddQuest("ancientchest_quest_id")
    ~ AddItem("ancientchest_id")
    -> END

=== GetNothing ===
#from :Player
เกลือว่ะเส้าอะ
-> END