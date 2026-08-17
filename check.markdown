# 淳平机械体实现检查清单

本文档用于交给另一个 AI 或开发者进行代码审查。审查目标不是重新设计技能，而是确认：

1. 用户需求是否全部覆盖。
2. XML、C#、配方、研究和培育器之间的引用是否完整。
3. 当前实现是否符合 RimWorld 1.6 + Biotech 的 API 和运行逻辑。
4. 数值、目标限制、持续时间和触发条件是否符合需求。
5. 是否存在只通过编译但进入游戏后才会暴露的问题。

## 1. 项目环境

- 游戏版本：RimWorld 1.6
- DLC：Biotech
- MOD 包 ID：`Adou.ChunPingMech`
- C# 目标框架：`netstandard2.1`
- 编译输出：`Assemblies/ChunPingMech.dll`
- 原版机械体贴图仍主要作为占位资源。

## 2. 主要文件

### XML

- `Defs/Abilities.xml`：所有主动 AbilityDef。
- `Defs/Hediff.xml`：技能 Buff、Debuff、隐身和被动状态。
- `Defs/AdvancedChunPingMech.xml`：标准机械体，包括三个恶霸淳平版本、患者和警察。
- `Defs/HyperChunPingMech.xml`：早稻田英雄、绿巨人、雪之武士和淳大哥。
- `Defs/UltraChunPingMech.xml`：雷奥、贝尔泽布布、魔王，以及召唤用的德川、我修院和德川诱饵。
- `Defs/SpecialThings.xml`：做局陷阱 ThingDef。
- `Defs/RecipeDef.xml`：机械培育配方。
- `Defs/Research.xml`：四级淳平机械科技研究。
- `Defs/Weapon.xml`：棒球棍、雪之太刀和强化 EMP 发射器。
- `Patches/MechGestatorRecipes.xml`：将培育配方注入普通或大型机械培育器。
- `Defs/JobDefs.xml`：患者冲刺爆破使用的 JobDef。

### C#

- `Source/ChunPingMech/ChunPingDashExplosion.cs`：冲刺爆破 Job 和爆炸逻辑。
- `Source/ChunPingMech/WasedaHeroAbilities.cs`：早稻田英雄全部主动技能和死亡/重伤被动。
- `Source/ChunPingMech/SnowSamuraiAbilities.cs`：雪之武士处决逻辑。
- `Source/ChunPingMech/AdvancedCombatAbilities.cs`：细胞重组、召唤、EMC、陷阱、诱饵和敌人标记。

## 3. 已实现功能

## 3.1 恶霸淳平：霸凌

### 需求

攻击型恶霸淳平能够使用“霸凌”，控制或严重削弱敌方目标。

### 当前实现

- AbilityDef：`ChunPing_Ability_Bullying`。
- 使用原版 `CompAbilityEffect_GiveHediff`。
- 目标类型：Pawn、Human、Animal；不能选择机械体或建筑。
- 使用 `Verb_CastAbilityTouch` 和 `CastAbilityOnThingMelee`，会接近目标后施法。
- 施加 Hediff：`ChunPing_Sumimasen`。
- Hediff 将目标的 `Consciousness` 最大值设为 `0.1`。
- Hediff 持续 `12000` ticks，并以 `-5.0/day` 衰减。
- 棒球棍版和枪械版都拥有该技能。

### 审查重点

- `Consciousness` 的 `setMax=0.1` 是否会导致目标立即倒地或产生非预期状态。
- Ability 的目标限制是否符合“霸凌”原始设计。
- `ChunPing_Sumimasen` 是否应该允许机械体作为目标。

## 3.2 患者淳平：冲刺爆破

### 需求

选择一个 location，患者快速冲刺到目标位置，到达后爆炸。

### 当前实现

- AbilityDef：`ChunPing_Ability_DashExplosion`。
- 目标：地图位置，不是 Pawn。
- 最大范围：`24.9`。
- 不要求视线，但要求目标位置在地图内且可到达。
- 施法时添加自身 Hediff：`ChunPing_DashRush`。
- `ChunPing_DashRush`：移动速度 `+3.6`，持续 `900` ticks。
- C# 组件：`CompAbilityEffect_DashExplosion`。
- 组件创建 Job：`ChunPing_DashExplosion`。
- Job 使用 `Toils_Goto.GotoCell` 移动到指定位置。
- 到达后执行 `GenExplosion.DoExplosion`。
- 爆炸类型：`DamageDefOf.Bomb`。
- 爆炸半径：`3.0`。
- 爆炸伤害：`55`。
- 爆炸实例以患者作为 instigator。

### 审查重点

- 路径被阻挡、目标点被占用或 Job 中断时是否会错误爆炸或不爆炸。
- 爆炸是否会伤害友军和患者自身，是否符合自爆单位设计。
- 施法位置、目标位置和爆炸位置在 Pawn 被击倒时是否仍安全处理。

## 3.3 警察淳平：广州警察

### 需求

提高自身射击命中率、射速、移速和射程；可增加合理的开枪相关 Buff。

### 当前实现

- AbilityDef：`ChunPing_Ability_GuangZhouPolice`。
- 这是无目标、自我施放的 Ability。
- 冷却：`7200` ticks。
- Buff Hediff：`ChunPing_GuangZhouPolice`。
- Buff 持续：`1800` ticks。
- `ShootingAccuracyPawn +4`。
- `AimingDelayFactor -0.25`，用于缩短瞄准时间。
- `MoveSpeed +0.75`。
- `Sight` capacity `+0.2`，用于间接改善远距离射击表现。
- 警察基础 `ShootingAccuracyPawn=11`，武器标签为 `MechanoidGunMedium`。

### 审查重点

- 当前没有直接修改武器最大射程的 stat；“射程增加”是通过 Sight 间接改善，需确认是否符合实际目标。
- `AimingDelayFactor` 使用负 stat offset 是否符合 RimWorld 1.6 的计算方式。
- 射速需求是通过瞄准延迟实现，还是需要额外修改武器 cooldown。

## 3.4 绿巨人淳平：机械盾卫式护盾

### 需求

参考原版机械盾卫，为绿巨人增加护盾。

### 当前实现

- 没有新增 C# 代码，直接使用原版 `CompProperties_ProjectileInterceptor`。
- 护盾半径：`4`。
- 拦截地面投射物：开启。
- 护盾生命：`280`。
- 充能后立即恢复满生命。
- 重新充能时间：`9000` ticks。
- EMP 禁用时间：`3000` ticks。
- 激活和环境音效使用原版护盾资源。
- 绿巨人基础生命倍率：`2.2`。
- 绿巨人基础锐器护甲：`1.6`。
- 绿巨人基础钝器护甲：`1.5`。

### 审查重点

- `CompProperties_ProjectileInterceptor` 的原版字段是否在 Biotech 1.6 中保持兼容。
- `hitPointsRestoreInstantlyAfterCharge=true` 是否过强。
- 护盾是否只拦截投射物，是否需要覆盖爆炸、近战或特殊伤害。

## 3.5 早稻田英雄淳平

### 3.5.1 英雄救场

#### 需求

选择一个友军，瞬移到友军身边；友军受到伤害减半；早稻田英雄获得“英雄主义”，移速提高到 150%，受到伤害减半。

#### 当前实现

- AbilityDef：`ChunPing_Ability_HeroRescue`。
- 目标必须是同地图、存活且非敌对 Pawn。
- 不允许选择自己。
- 在友军半径 `2` 内查找可站立且没有 Pawn 占用的位置。
- C# 直接设置施法者的 `Position`，然后调用 `Notify_Teleported()`。
- 友军获得 `ChunPing_HeroProtection`，持续 `900` ticks。
- `HeroProtection.IncomingDamageFactor=0.5`。
- 施法者获得 `ChunPing_Heroism`，持续 `900` ticks。
- `Heroism.MoveSpeed=1.5`。
- `Heroism.IncomingDamageFactor=0.5`。

#### 审查重点

- 代码提示语写“未倒地的友军”，但 `IsFriendly` 当前没有显式排除 `Downed` Pawn，需要确认是否为问题。
- 直接设置 Position 是否应改为安全传送 API 或处理占用、屋顶和不可通行位置。
- “保护友军”当前是减伤，并不是将伤害转移给英雄。

### 3.5.2 治愈

#### 需求

选择友军，为其包扎所有伤口。

#### 当前实现

- AbilityDef：`ChunPing_Ability_HeroHeal`。
- 目标必须是友军，不能选择自己。
- 只有存在 `TendableNow()` 伤口时才能施放。
- 遍历目标所有 Hediff，对所有可包扎伤口调用 `Tended(1f, 1f, 1)`。
- 完成后显示包扎数量文本。

#### 审查重点

- 该实现是“完成包扎”，不是直接把伤口 Severity 清零；确认这是否满足“包扎所有伤口”。
- 不能处理不可包扎的永久伤、缺失肢体或其他非 `TendableNow` 状态。
- 遍历并修改 Hediff 列表时的倒序遍历是否仍然安全。

### 3.5.3 伤害转移

#### 需求

选择友军，将其所有伤势转移到早稻田英雄身上。

#### 当前实现

- AbilityDef：`ChunPing_Ability_DamageTransfer`。
- 目标必须是友军且至少有一个 `Hediff_Injury`。
- 收集友军所有 `Hediff_Injury`。
- 从友军移除每个伤势。
- 在英雄身上创建同一个伤势 Def，并复制 Severity。
- 尝试匹配相同的身体部位 Def；如果英雄没有对应部位，则使用核心身体部位。

#### 审查重点

- 转移到英雄后是否可能因为累计伤势直接死亡或倒地。
- 肢体结构不同、身体部位不存在时使用核心部位是否合理。
- 当前只转移 `Hediff_Injury`，不会转移疾病、烧伤以外的状态、缺失肢体或其他伤势类型。
- 源 Pawn 被移除伤势后，目标 Pawn AddHediff 是否可能触发死亡检查或异常。

### 3.5.4 早稻田精神

#### 需求

所有友军移动速度 `130%`，受到伤害 `80%`，疼痛为 `0`，近战和远程命中增加 `20%`。

#### 当前实现

- AbilityDef：`ChunPing_Ability_WasedaSpirit`。
- 无目标，作用于施法者所在地图。
- 对所有同地图、非敌对 Pawn 施加 `ChunPing_WasedaSpirit`，包括施法者。
- 持续 `900` ticks。
- `MoveSpeed=1.3`。
- `IncomingDamageFactor=0.8`。
- `painFactor=0`。
- `MeleeHitChance=1.2`。
- `ShootingAccuracyPawn=1.2`。

#### 审查重点

- “命中 +20%”是否应使用乘数 `1.2`，还是使用 stat offset `+20`。
- 地图上的中立 Pawn 是否应被算作友军；当前逻辑是“不敌对”即算友军。
- 是否需要排除倒地、囚犯、动物或临时召唤物。

### 3.5.5 英雄不死于无名之处

#### 需求

生命严重受损时触发一次，附近友军获得移速、射击命中、近战命中和疼痛减免，持续 15 秒。

#### 当前实现

- 早稻田英雄 ThingDef 挂载 `CompWasedaHeroPassive`。
- 严重受损阈值：`SummaryHealthPercent <= 0.3`。
- 使用 `lastStandTriggered` 保证每个英雄只触发一次。
- 范围：半径 `12`。
- 附近友军获得 `ChunPing_HeroLastStand`。
- 持续 `900` ticks，即 15 秒。
- `MoveSpeed=1.3`。
- `MeleeHitChance=1.25`。
- `ShootingAccuracyPawn=1.25`。
- `painFactor=0.5`。

#### 审查重点

- 需求只说“疼痛降低”，当前是疼痛乘数 `0.5`，不是完全消除疼痛。
- 触发依据是总结生命比例，不是某次伤害事件的严重受伤判定。
- 低血量检测同时在 `CompTickRare` 和受伤回调触发，需确认不会重复触发。

### 3.5.6 死亡后的英雄遗志

#### 需求

早稻田英雄死亡时，附近友军获得“英雄的遗志”，短暂提高战斗能力。

#### 当前实现

- 英雄身上自动维护 `ChunPing_HeroDeathTrigger`。
- 该 Hediff 包含 `HediffComp_HeroDeathLegacy`。
- 通过 `Notify_PawnDied` 触发一次。
- 对附近半径 `12` 的友军施加 `ChunPing_HeroLegacy`。
- `HeroLegacy` 持续 `900` ticks。
- `MoveSpeed=1.25`。
- `IncomingDamageFactor=0.75`。
- `MeleeHitChance=1.3`。
- `ShootingAccuracyPawn=1.3`。
- `AimingDelayFactor=0.75`。
- `painFactor=0.5`。

#### 审查重点

- `MapHeld` 和 `PositionHeld` 在死亡通知期间是否始终有效。
- 死亡后触发范围是否应包含死亡位置附近的倒地或临时单位。
- `HeroDeathTrigger` 是否会在存档读档后正确保持触发状态。

## 3.6 雪之武士淳平

### 3.6.1 隐身

- AbilityDef：`ChunPing_Ability_SnowInvisibility`。
- 自我施放。
- 使用 `HediffCompProperties_Invisibility`。
- 持续 `900` ticks。
- `visibleToPlayer=true`，因此玩家仍能看到自身单位。

审查重点：确认该原版隐身组件是否会影响敌人目标搜索、射击、近战和绘制效果。

### 3.6.2 雪之呼吸

- AbilityDef：`ChunPing_Ability_SnowBreath`。
- 自我施放，持续 `900` ticks。
- `MoveSpeed=1.5`。
- `MeleeCooldownFactor=0.65`，即近战攻击间隔缩短。

审查重点：需求中的“攻速”当前按近战攻击速度实现，没有修改远程武器射速。

### 3.6.3 处决

- AbilityDef：`ChunPing_Ability_Execution`。
- 目标必须是敌方 Pawn。
- 使用 `Verb_CastAbilityTouch`，自动接近目标。
- 敌人生命比例 `< 0.1` 时使用 `DamageDefOf.ExecutionCut` 直接击杀。
- 否则使用 `DamageDefOf.Cut`，伤害 `70`，护甲穿透 `1.5`。

审查重点：当前判断是严格小于 10%，等于 10% 不会处决；需要确认是否应使用小于等于。

## 3.7 雷奥淳平：细胞重组

### 需求

短时间内快速治愈目标的伤势。

### 当前实现

- AbilityDef：`ChunPing_Ability_CellRecombination`。
- 当前解释为“选择有伤势的友军”，不能选择敌人或自己。
- 持续 `900` ticks，即 15 秒。
- Buff Hediff：`ChunPing_CellRecombination`。
- 每 `60` ticks 治疗一次。
- 每个 `Hediff_Injury` 每次恢复 `2` Severity。
- C# 组件：`HediffComp_CellRecombination`。

### 审查重点

- 用户原文使用“对方”，当前实现将其解释为友军；确认是否应该允许所有 Pawn。
- 当前治疗的是 `Hediff_Injury`，不会处理疾病、永久伤、缺失身体部位等。
- 15 秒内每个伤口最多大约恢复 `30` Severity，数值是否过强或不足需要实战平衡。
- Hediff 列表在组件 Tick 中倒序遍历并直接 Heal，确认伤口完全消失时不会产生迭代问题。

## 3.8 贝尔泽布布淳平：召唤机械体

### 需求

参考原版战争女皇，拆分为两个技能，分别释放德川和我修院机械体。

### 两个 Ability

- `ChunPing_Ability_ReleaseTokugawa`：释放德川。
- `ChunPing_Ability_ReleaseIshuin`：释放我修院。
- 两个技能都选择地图位置。
- C# 共用 `CompAbilityEffect_SummonMech`。
- 在目标位置半径约 `2.9` 内寻找可站立且没有 Pawn 的格子。
- 通过 `PawnGenerator.GeneratePawn` 生成单位。
- 使用施法者的 Faction。
- 生成后直接 Spawn 到地图。
- 召唤物不占用控制带宽。
- 普通召唤物寿命：`7200` ticks，即 120 秒。
- 召唤物 `hasCorpse=false`。

### 德川机械体

- Thing/Pawn Def：`Mech_ChunPing_Tokugawa`。
- 使用 `WarUrchinConstant` 自主战斗思考树。
- 轻型快速近战机械体。
- `MoveSpeed=5.4`。
- `baseHealthScale=0.42`。
- 近战工具伤害 `14`，冷却 `1.7`。
- 战斗力：`75`。

### 我修院机械体

- Thing/Pawn Def：`Mech_ChunPing_Ishuin`。
- 使用 `WarUrchinConstant` 自主战斗思考树。
- 轻型快速短程射击机械体。
- `MoveSpeed=4.8`。
- `baseHealthScale=0.38`。
- 武器标签：`MechanoidGunShortRange`。
- 战斗力：`80`。

### 审查重点

- `WarUrchinConstant` 在当前 Biotech/Odyssey 环境中是否存在且适用于这些 PawnKind。
- 召唤单位是否会正确识别敌我、自动攻击和死亡。
- 召唤单位是否应该有召唤者归属、控制关系或最大同时数量限制。
- 当前没有战争女皇原版的资源消耗、召唤动画或特殊召唤特效，这是有意简化还是遗漏。

## 3.9 淳大哥：EMC

### 需求

在指定范围内爆发一次大规模 EMC。

### 当前实现

- AbilityDef：`ChunPing_Ability_EMC`。
- 目标是地图位置。
- 目标位置只要求在地图内，不要求可站立，允许对墙体或建筑位置释放。
- 使用 `GenExplosion.DoExplosion`。
- 实际伤害类型为 `DamageDefOf.EMP`。
- 爆炸半径：`8`。
- EMP 伤害参数：`45`。
- C# 组件：`CompAbilityEffect_AreaEmp`。

### 审查重点

- 用户写的是 EMC，代码实际使用的是 EMP；确认是否只是在命名上使用 EMC，效果是否符合预期。
- EMP 爆炸对机械体、炮塔、护盾和设备的实际作用要在游戏内验证。
- 是否需要视线限制、敌我过滤或友军保护。

## 3.10 淳大哥：做局陷阱

### 需求

在指定位置部署隐蔽装置，敌人进入范围后自动触发。

### 当前实现

- AbilityDef：`ChunPing_Ability_SchemeTrap`。
- 目标为地图位置，要求位置可站立且没有建筑。
- 生成 Thing：`ChunPing_SchemeTrap`。
- Thing 使用 `Building` 类和 `Rare` ticker。
- 绘制尺寸：`0.45`，使用 `Things/Building/Security/IEDTrap` 贴图作为占位。
- 由施法者设置陷阱 Faction。
- `CompProximityTrap` 每次低频 Tick 检查地图 Pawn。
- 只有 `pawn.HostileTo(parent)` 的 Pawn 会触发陷阱。
- 触发半径：`3.5`。
- 触发后使用 `DamageDefOf.Bomb` 爆炸。
- 爆炸半径：`4`。
- 爆炸伤害：`45`。
- 护甲穿透：`0.5`。
- 爆炸后销毁陷阱。

### 审查重点

- `Things/Building/Security/IEDTrap` 是否是当前游戏版本存在的贴图路径。
- `Rare` ticker 的检查间隔是否导致敌人能够在陷阱触发前穿过范围。
- 陷阱是否可被敌人或玩家发现、选择、攻击和拆除；当前“隐蔽”主要通过小尺寸实现。
- 爆炸是否会伤害施法者阵营，以及这是否符合设计。

## 3.11 淳大哥：德川诱饵

### 需求

在指定地点生成一个攻击力很低但仇恨很高的诱饵单位。

### 当前实现

- AbilityDef：`ChunPing_Ability_TokugawaDecoy`。
- 使用通用召唤组件生成 `Mech_ChunPing_TokugawaDecoy`。
- 目标位置附近寻找可用格子。
- 诱饵寿命：`3600` ticks，即 60 秒。
- `WarUrchinConstant` 自主思考树。
- 诱饵的近战工具伤害：`2`。
- 战斗力：`15`。
- `CompHighAggroDecoy` 每 60 ticks 扫描一次半径 `24` 内敌人。
- 对附近敌人的 `mindState.enemyTarget` 直接设置为诱饵。
- 诱饵不留下尸体。

### 审查重点

- 设置 `enemyTarget` 是否足以让所有敌人切换攻击目标；某些敌人可能继续执行当前 Job。
- 是否应该通过 Job、攻击目标评分或自定义 ThinkNode 实现更可靠的仇恨。
- 当前诱饵会影响所有对其 Hostile 的 Pawn，包括非人类单位和其他派系单位，需要确认范围。
- 诱饵没有单独的视觉、声音或挑衅特效。

## 3.12 淳大哥：我修院标记

### 需求

指定一个敌人，使其受到伤害 `150%`，移动速度 `80%`。

### 当前实现

- AbilityDef：`ChunPing_Ability_IshuinMark`。
- 目标必须是同地图、存活且敌对的 Pawn。
- 施加 `ChunPing_IshuinMark`。
- 持续 `1200` ticks，即 20 秒。
- `IncomingDamageFactor=1.5`。
- `MoveSpeed=0.8`。
- 重复施放会先移除旧 Hediff，再重新添加并刷新持续时间。

### 审查重点

- `IncomingDamageFactor` 是否会与护甲、护盾和其他伤害乘区按预期叠加。
- 是否允许标记机械体、动物和人类；当前三者都允许。
- 是否需要施法视线或特殊抗性。

## 3.13 恶霸淳平（盾）

### 需求

增加一个名为“恶霸淳平（盾）”的机械体，不增加武器和技能，主要作用是拥有非常高的护甲。

### 当前实现

- Thing/Pawn Def：`Mech_ChunPing_Bully_Shield`。
- 标准机械科技单位。
- `ArmorRating_Sharp=2.2`。
- `ArmorRating_Blunt=2.4`。
- `ArmorRating_Heat=1.5`。
- `baseHealthScale=2.2`。
- `baseBodySize=1.45`。
- `MoveSpeed=1.8`。
- `mechWeightClass=Heavy`。
- 显式清空 ThingDef 的 `tools`。
- 显式清空 ThingDef 的 `abilities`。
- 显式清空 PawnKindDef 的 `weaponTags`。
- 没有新增 C# 逻辑。
- 已增加 `Gestate_ChunPing_Bully_Shield` 配方。
- 已通过 `Patches/MechGestatorRecipes.xml` 加入 `LargeMechGestator`。
- 使用 Fabricor 占位贴图，绘制尺寸 `1.25`。

### 审查重点

- 清空 `tools` 后是否完全没有近战攻击，是否符合“不需要增加武器”的含义。
- `isFighter=true` 但没有武器和工具时，AI 是否会出现无效战斗 Job。
- `mechWeightClass=Heavy` 与标准机械培育配方是否兼容。
- 220%/240% 护甲和 2.2 生命倍率是否超出标准科技的合理强度。

## 3.14 工程、构建与文档调整

### 当前实现

- `Source/ChunPingMech/ChunPingMech.csproj` 以 `netstandard2.1` 编译。
- 工程引用本地 RimWorld 1.6 的 `Assembly-CSharp.dll`、`UnityEngine.dll` 和 `UnityEngine.CoreModule.dll`。
- 编译输出目录设置为 MOD 根目录的 `Assemblies/`。
- 已生成 `Assemblies/ChunPingMech.dll`。
- 新增 `Defs/JobDefs.xml`，用于注册患者冲刺爆破 JobDriver。
- 新增 `Defs/SpecialThings.xml`，用于注册做局陷阱。
- `ReadMe.markdown` 已从旧的“第二阶段开发规格”重写为当前实现状态。
- README 当前记录 20 种可培育机械体、3 种临时机械体、17 个主动技能、项目结构、构建命令、当前限制和后续优先级。

### 审查重点

- `.csproj` 的本地程序集 HintPath 是否只在当前开发机器有效；发布源码后其他开发者可能无法直接编译。
- `Assemblies/ChunPingMech.dll` 是否由最新源码生成，时间戳和实际类型是否一致。
- `ReadMe.markdown` 中的单位数量、技能数量和状态是否与 Def 保持一致。
- `About/About.xml` 的描述可能仍包含旧的“主动技能尚未实现”信息，需要检查是否也应同步更新。

## 4. 交叉引用检查

另一个 AI 应逐项确认以下链路：

### 主动技能链

```text
ThingDef.abilities
  -> AbilityDef.defName
  -> AbilityDef.comps
  -> Class / compClass
  -> HediffDef、PawnKindDef 或 ThingDef
```

### 可培育机械体链

```text
ThingDef.defName
  -> PawnKindDef.race
  -> RecipeDef.products
  -> RecipeDef.descriptionHyperlinks
  -> MechGestator 或 LargeMechGestator recipes Patch
  -> 研究项目前置条件
```

### 当前需要重点核对的 DefName

```text
ChunPing_Ability_DashExplosion
ChunPing_Ability_GuangZhouPolice
ChunPing_Ability_HeroRescue
ChunPing_Ability_HeroHeal
ChunPing_Ability_DamageTransfer
ChunPing_Ability_WasedaSpirit
ChunPing_Ability_SnowInvisibility
ChunPing_Ability_SnowBreath
ChunPing_Ability_Execution
ChunPing_Ability_CellRecombination
ChunPing_Ability_ReleaseTokugawa
ChunPing_Ability_ReleaseIshuin
ChunPing_Ability_EMC
ChunPing_Ability_SchemeTrap
ChunPing_Ability_TokugawaDecoy
ChunPing_Ability_IshuinMark
```

```text
ChunPing_CellRecombination
ChunPing_IshuinMark
ChunPing_SchemeTrap
Mech_ChunPing_Tokugawa
Mech_ChunPing_Ishuin
Mech_ChunPing_TokugawaDecoy
Mech_ChunPing_Bully_Shield
Gestate_ChunPing_Bully_Shield
```

## 5. 代码级检查点

- XML 中的 `Class` 和 `compClass` 是否都能在 `ChunPingMech.dll` 中找到。
- 所有 `CompProperties` 是否在构造函数中设置了正确的 `compClass`。
- `CompAbilityEffect.Valid` 与 `CanApplyOn` 是否使用一致的目标限制。
- 所有目标 Pawn 是否检查了 `Dead`、`Spawned`、地图一致性和敌我关系。
- 所有地图位置是否检查了 `InBounds`。
- 所有传送和召唤位置是否检查了可站立、Pawn 占用和建筑冲突。
- Hediff 持续时间是否使用 ticks，且 `HediffComp_Disappears` 能正确接收运行时覆盖时间。
- 伤势遍历时是否避免修改列表造成跳过或重复处理。
- 召唤物死亡、到期和存档读档是否安全。
- `PostExposeData` 中的状态字段是否正确保存和恢复。
- 陷阱和诱饵是否会在地图卸载、Pawn 死亡或 Thing 销毁后继续访问无效对象。
- EMP、Bomb 和 ExecutionCut 的伤害类型是否在当前游戏版本存在。

## 6. 已执行验证

当前已执行：

- 使用 PowerShell `XmlDocument` 解析全部 `Defs/*.xml`。
- 使用 PowerShell `XmlDocument` 解析 `Patches/*.xml`。
- 执行 `dotnet build .\Source\ChunPingMech\ChunPingMech.csproj --no-restore`。
- 最近一次编译结果：`0` warnings，`0` errors。
- 执行 `git diff --check`，没有发现空白错误；仅有 Git 的换行符提示。
- 已检查盾恶霸的 ThingDef、PawnKindDef、RecipeDef 和大型培育器 Patch 都存在。
- 已检查盾恶霸的 `tools`、`abilities` 和 `weaponTags` XML 列表为空。

尚未完成或需要在游戏内执行：

- RimWorld 启动日志实际加载测试。
- 所有 Ability 的 Gizmo、目标选择和冷却实测。
- 召唤物自主攻击和诱饵仇恨切换实测。
- 陷阱贴图、隐蔽性和触发时机实测。
- 护盾对投射物、EMP 和重型武器的实测。
- 伤势转移、细胞重组和死亡被动的极端情况测试。
- 视觉资源、音效和数值平衡测试。

## 7. 审查结论格式建议

另一个 AI 检查时，请按以下格式报告：

```text
问题等级：严重 / 高 / 中 / 低
位置：文件路径 + 行号或 DefName/Class 名称
对应需求：用户原始需求
实际行为：当前代码或 XML 的具体行为
问题原因：为什么不符合需求、API 或游戏逻辑
建议修复：最小修改方案
是否需要游戏内测试：是 / 否
```

如果没有发现问题，也需要明确列出：

1. XML 解析是否通过。
2. C# 编译是否通过。
3. Def 交叉引用是否通过。
4. 哪些问题只能通过 RimWorld 实际启动和战斗测试确认。
