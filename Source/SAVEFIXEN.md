# How to fix a old save.

## 1. Open the file of the save which you want to fix.
## 2. Search **FOU_MatterDisintegration**.
>Search result should be like
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
## 3. Replace **Verb_CastAbilityTouch** as **ForeignerOfUniverse.Verbs.CastAbilityDirectly**.
>Result
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
## 4. Replace all search results from step 2.
## 4. Save file, fix finished.