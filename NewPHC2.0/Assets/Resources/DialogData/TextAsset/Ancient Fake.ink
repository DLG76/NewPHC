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
    -> GetNothing
+ [ไม่ทำอะไร]
    -> END

=== GetNothing ===
#from :Player
เกลือว่ะเส้าอะ
-> END