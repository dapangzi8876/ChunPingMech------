# 淳平机械体

RimWorld 1.6 + Biotech 机械体 MOD。

当前版本已经完成基础机械体框架、四级研究与培育链，并为主要战斗单位加入了主动技能、Hediff、召唤、陷阱、护盾和自定义战斗逻辑。项目仍使用原版机械体贴图作为占位资源。

## 依赖与版本

- RimWorld 1.6
- Biotech DLC
- 包 ID：`Adou.ChunPingMech`
- C# 程序集：`Assemblies/ChunPingMech.dll`

## 当前规模

- 20 种可研究、可培育的淳平机械体
- 3 种技能生成的临时机械体
- 4 个机械科技研究项目
- 普通机械培育器负责基础机械体
- 大型机械培育器负责标准、高级和终极机械体

## 科技与单位

### 基础机械科技

| 单位 | DefName | 当前定位 |
| --- | --- | --- |
| 书法家淳平 | `Mech_ChunPing_Calligrapher` | 艺术工作 |
| 服务员淳平 | `Mech_ChunPing_Waiter` | 基础工作、烹饪、清洁 |
| 土木淳平 | `Mech_ChunPing_Construction` | 建筑、采矿 |
| 剑道部淳平 | `Mech_ChunPing_Kendo` | 制造、搬运、基础近战 |
| 园丁淳平 | `Mech_ChunPing_Gardener` | 种植、伐木、搬运 |
| 汽车修理工淳平 | `Mech_ChunPing_Mechanic` | 制造、锻造、缝纫 |
| 黄毛淳平 | `Mech_ChunPing_Blond` | 狩猎与基础远程攻击 |

### 标准机械科技

| 单位 | DefName | 当前定位与机制 |
| --- | --- | --- |
| 大医生淳平 | `Mech_ChunPing_Doctor` | 医疗与科研 |
| 恶霸淳平（棒球棍） | `Mech_ChunPing_Bully_ballBat` | 棒球棍近战；拥有“霸凌” |
| 恶霸淳平（枪械） | `Mech_ChunPing_Bully_Gun` | 中距离射击；拥有“霸凌” |
| 恶霸淳平（盾） | `Mech_ChunPing_Bully_Shield` | 无专用武器、无技能；极高护甲和生命，低移速 |
| 患者淳平 | `Mech_ChunPing_Patient` | “冲刺爆破”：冲向指定地点并爆炸 |
| 警察淳平 | `Mech_ChunPing_Police` | “广州警察”：强化命中、射速、移速和射程 |

### 高级机械科技

| 单位 | DefName | 当前定位与机制 |
| --- | --- | --- |
| 早稻田英雄淳平 | `Mech_ChunPing_WasedaHero` | 英雄救场、治愈、伤害转移、早稻田精神、重伤与死亡被动 |
| 绿巨人淳平 | `Mech_ChunPing_Hulk` | 高生命重装坦克；拥有机械盾卫式弹丸拦截护盾 |
| 雪之武士淳平 | `Mech_ChunPing_SnowSamurai` | 隐身、雪之呼吸、低生命目标处决 |
| 淳大哥 | `Mech_ChunPing_BigBrother` | EMC、做局陷阱、德川诱饵、我修院易伤减速标记 |

### 终极机械科技

| 单位 | DefName | 当前定位与机制 |
| --- | --- | --- |
| 雷奥淳平 | `Mech_ChunPing_Leo` | 超重型近战；“细胞重组”持续快速治愈友军 |
| 贝尔泽布布淳平 | `Mech_ChunPing_Beelzebub` | 重型射击与召唤；可释放德川和我修院 |
| 魔王淳平 | `Mech_ChunPing_DemonKing` | 超重型远程炮台；特殊炮击机制尚未实现 |

## 临时机械体

以下单位不能通过培育器制造，只由技能生成：

| 单位 | DefName | 机制 |
| --- | --- | --- |
| 德川机械体 | `Mech_ChunPing_Tokugawa` | 自主近战，120 秒后失效 |
| 我修院机械体 | `Mech_ChunPing_Ishuin` | 自主短程射击，120 秒后失效 |
| 德川诱饵 | `Mech_ChunPing_TokugawaDecoy` | 低攻击、高仇恨，60 秒后失效 |

临时机械体使用原版 `WarUrchinConstant` 思考树自主战斗，不占用控制带宽，到期后不留下尸体。

## 已实现技能

| AbilityDef | 显示名称 | 使用者 |
| --- | --- | --- |
| `ChunPing_Ability_Bullying` | 霸凌 | 两种攻击型恶霸淳平 |
| `ChunPing_Ability_DashExplosion` | 冲刺爆破 | 患者淳平 |
| `ChunPing_Ability_GuangZhouPolice` | 广州警察 | 警察淳平 |
| `ChunPing_Ability_HeroRescue` | 英雄救场 | 早稻田英雄淳平 |
| `ChunPing_Ability_HeroHeal` | 治愈 | 早稻田英雄淳平 |
| `ChunPing_Ability_DamageTransfer` | 伤害转移 | 早稻田英雄淳平 |
| `ChunPing_Ability_WasedaSpirit` | 早稻田精神 | 早稻田英雄淳平 |
| `ChunPing_Ability_SnowInvisibility` | 隐身 | 雪之武士淳平 |
| `ChunPing_Ability_SnowBreath` | 雪之呼吸 | 雪之武士淳平 |
| `ChunPing_Ability_Execution` | 处决 | 雪之武士淳平 |
| `ChunPing_Ability_CellRecombination` | 细胞重组 | 雷奥淳平 |
| `ChunPing_Ability_ReleaseTokugawa` | 释放德川 | 贝尔泽布布淳平 |
| `ChunPing_Ability_ReleaseIshuin` | 释放我修院 | 贝尔泽布布淳平 |
| `ChunPing_Ability_EMC` | EMC | 淳大哥 |
| `ChunPing_Ability_SchemeTrap` | 做局陷阱 | 淳大哥 |
| `ChunPing_Ability_TokugawaDecoy` | 德川诱饵 | 淳大哥 |
| `ChunPing_Ability_IshuinMark` | 我修院 | 淳大哥 |

## 研究与培育链

研究项目：

```text
ChunPing_BasicMechtech
  -> ChunPing_StandardMechtech
  -> ChunPing_HighMechtech
  -> ChunPing_UltraMechtech
```

配方父类：

```text
ChunPingBasicRecipe
ChunPingStandardRecipe
ChunPingHighRecipe
ChunPingUltraRecipe
```

新增可培育机械体时需要同时维护：

1. `ThingDef`
2. `PawnKindDef`
3. `RecipeDef`
4. `Patches/MechGestatorRecipes.xml`
5. 对应研究项目

## 项目结构

```text
Defs/BasicChunPingMech.xml       基础机械体
Defs/AdvancedChunPingMech.xml    标准机械体
Defs/HyperChunPingMech.xml       高级机械体
Defs/UltraChunPingMech.xml       终极与临时机械体
Defs/Abilities.xml               主动技能
Defs/Hediff.xml                  Buff、Debuff 与被动状态
Defs/SpecialThings.xml           做局陷阱等特殊 Thing
Defs/Weapon.xml                  专属武器
Defs/RecipeDef.xml               培育配方
Defs/Research.xml                研究项目
Patches/MechGestatorRecipes.xml  培育器配方注入
Source/ChunPingMech/              C# 技能与战斗逻辑
Assemblies/ChunPingMech.dll      编译后的程序集
```

## 构建与检查

C# 构建命令：

```powershell
dotnet build .\Source\ChunPingMech\ChunPingMech.csproj --no-restore
```

每次修改后至少检查：

- 所有 Def XML 可以被 XML 解析器读取
- 没有重复 `defName`
- Ability、Hediff、PawnKind、Thing 和 Recipe 交叉引用存在
- `PawnKindDef.lifeStages` 与种族生命阶段数量一致
- C# 中的类名与 XML `Class`/`compClass` 完全一致
- RimWorld 启动日志没有缺失 Def、贴图或类型错误

## 当前限制

- 所有正式机械体仍使用原版 Fabricor 贴图作为占位资源；临时召唤物使用战争小镰贴图。
- 音效、技能特效和图标大多仍使用原版资源。
- 技能数值只完成第一轮配置，仍需要在实际战斗中测试和平衡。
- 尚未实现敌对淳平袭击、Boss 事件和完整特殊 AI。
- 魔王淳平的魔王炮、蓄力炮击与部署模式尚未实现。

## 后续优先级

1. 在 RimWorld 内完成全部主动技能、召唤物和陷阱的启动日志与实战测试。
2. 为魔王淳平实现部署、蓄力炮击和范围攻击。
3. 为基础工作型机械体增加能够区分定位的被动或工作机制。
4. 制作并替换各机械体的正式贴图、技能图标、音效和视觉效果。
5. 开发敌对淳平派系、袭击事件与 Boss 内容。
