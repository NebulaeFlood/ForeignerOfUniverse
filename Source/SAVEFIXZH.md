# 如何修复旧存档

## 1. 打开要修复的存档文件
## 2. 搜索 **FOU_MatterDisintegration**
>搜索结果应类似
>```xml
><li>
>	<def>FOU_MatterDisintegration</def>
>	<Id>123</Id>
>	<sourcePrecept>null</sourcePrecept>
>	<verbTracker>
>		<verbs>
>			<li Class="Verb_CastAbilityTouch">
>				<loadID>Ability_123_0</loadID>
>				<lastShotTick>-999999</lastShotTick>
>				<ability>Ability_123</ability>
>			</li>
>		</verbs>
>	</verbTracker>
>	<maxCharges>0</maxCharges>
>	<charges>0</charges>
>	<lastCastTick>8270</lastCastTick>
>/li>
>```
## 3. 将 **verbs** 节点下的 **Verb_CastAbilityTouch** 修改为 **ForeignerOfUniverse.Verbs.CastAbilityDirectly**
>修改结果为
>```xml
><li>
>	<def>FOU_MatterDisintegration</def>
>	<Id>123</Id>
>	<sourcePrecept>null</sourcePrecept>
>	<verbTracker>
>		<verbs>
>			<li Class="ForeignerOfUniverse.Verbs.CastAbilityDirectly">
>				<loadID>Ability_123_0</loadID>
>				<lastShotTick>-999999</lastShotTick>
>				<ability>Ability_123</ability>
>			</li>
>		</verbs>
>	</verbTracker>
>	<maxCharges>0</maxCharges>
>	<charges>0</charges>
>	<lastCastTick>8270</lastCastTick>
>/li>
>```
## 4. 替换所有在步骤 2 中的搜索结果
## 5. 保存文件，修复完成