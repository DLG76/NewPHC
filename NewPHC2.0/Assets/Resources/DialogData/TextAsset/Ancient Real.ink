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
    -> Get
+ [ไม่ทำอะไร]
    -> END

=== Get ===
#from :Player
เห้ย เจอแล้ววววว
~ AddItem("universalkey_id")
-> END