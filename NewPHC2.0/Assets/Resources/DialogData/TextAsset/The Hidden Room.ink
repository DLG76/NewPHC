INCLUDE Global.ink

VAR SPEAKER_ID = ""
VAR SPEAKER_IDS = ""
{ SPEAKER_ID:
    - "" : -> Default
}

VAR QUEST_ID = ""
VAR REWARD_ID = ""

=== Default ===
หินอะไรอะดูน่ากลัวจัง...
+ [แตะ]
    ~ Teleport("SamuraiQuest")
    -> END
+ [ไม่ทำอะไร]
    -> END