using System.Collections.Generic;


    /// <summary>
    /// 戦闘中のユニットデータ（味方・敵共通）
    /// </summary>
    [System.Serializable]
    public class UnitCharacter
    {
        public int unitId;
        public int characterId;
        public string name;
        public int currentHp;
        public int currentMp;
        public int maxHp;
        public int maxMp;
        public int atk;
        public int def;
        public int speed;
        public float currentActionGauge;
        public bool isAlly;
        public List<int> skillIds;
        public int ultimateSkillId;

        /// <summary>
        /// シリアライゼーション用デフォルトコンストラクタ
        /// </summary>
        public UnitCharacter() { }

        /// <summary>
        /// 味方キャラクターからユニットを生成する
        /// </summary>
        public UnitCharacter(RuntimeCharacter runtime, int newUnitId)
        {
            unitId = newUnitId;
            characterId = runtime.id;
            name = runtime.name;
            maxHp = runtime.maxHp;
            currentHp = runtime.currentHp;
            maxMp = runtime.maxMp;
            currentMp = runtime.currentMp;
            atk = runtime.atk;
            def = runtime.def;
            speed = runtime.speed;
            isAlly = true;
            skillIds = new List<int>();
            if(runtime.skillId != null) skillIds.AddRange(runtime.skillId);
            ultimateSkillId = runtime.ultimateSkillId;
        }

        /// <summary>
        /// 敵キャラクターからユニットを生成する
        /// </summary>
        public UnitCharacter(EnemyMasterSO enemy, int newUnitId)
        {
            unitId = newUnitId;
            characterId = enemy.id;
            name = enemy.characterName;
            maxHp = enemy.hp;
            currentHp = enemy.hp;
            maxMp = enemy.mp;
            currentMp = enemy.mp;
            atk = enemy.atk;
            def = enemy.def;
            speed = enemy.speed;
            isAlly = false;
            skillIds = new List<int>();
            if(enemy.skillId != null) skillIds.AddRange(enemy.skillId);
            ultimateSkillId = enemy.ultimateSkillId;
        }

        /// <summary>
        /// 速度から次の行動までの時間を計算する
        /// </summary>
        /// <param name="goalDistance"></param>
        /// <returns></returns>
        public float GetRemainingTime(float goalDistance)
        {
            return (goalDistance - currentActionGauge) / speed;
        }

        /// <summary>
        /// 自身の次行動までの距離を指定分進める
        /// </summary>
        /// <param name="worldTime"></param>
        public void AdvanceGauge(float worldTime)
        {
            currentActionGauge += speed * worldTime;
        }
    }
