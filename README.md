# guide
- roll syntax is `roll <dice> <modifier> <repeats>`
- `<dice>` is the number and size of the dice to roll, e.g. `2d6`, `1d20`
- `<modifier>` is what to add at the end of the roll, e.g. `+5`, `-5`
- `<repeats>` is how many times to repeat the roll, e.g. `x2` for advantage, or `x3` for rolling three attacks at once

# examples
- `roll 1d20 +6` or `r 6` rolls a skill check with a +6 modifier
- `roll 2d6 +5 x3` or `2d6 5 x3` rolls greatsword damage with a +5 modifier three times
- `roll 1d20 1d4 -3 x2` or `d20 d4 -3 x` rolls a stealth check with a -3 modifier, guidance, and disadvantage

# advanced
- plus signs can be omitted
	- e.g. `roll 1d20 +6 x2` is the same as `roll 1d20 6 x2`
- if `<dice>` doesn't specify a count, it defaults to `1`
	- e.g. `roll 1d20 6 x2` is the same as `roll d20 6 x2`
- if `<dice>` is omitted entirely, it defaults to `1d20`
	- e.g. `roll d20 6 x2` is the same as `roll 6 x2`
- `roll` can be shortened to `r`
	- e.g. `roll 6 x2` is the same as `r 6 x2`
- `roll` or `r` can be omitted entirely if at least two parameters are provided
	- e.g. `r 6 x2` is the same as `6 x2`
- if `<repeats>` doesn't specify a count, it defaults to `2`
	- e.g. `6 x2` is the same as `6 x`

# saved rolls
- each user has their own set of saved rolls
- saving syntax is `save <name> <roll>`
- each time `<name>` appears in a roll command, it will be replaced by `<roll>`
- e.g. calling `save damage 2d6 +5` and then `roll damage x2` is the same as calling `roll 2d6 +5 x2`
- `save <name>` without `<roll>` deletes that saved roll if it exists
- `view <name>` views that saved roll if it exists
- `view` without `<name>` views all saved rolls for that user
