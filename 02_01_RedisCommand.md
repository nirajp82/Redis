
## Note 1:
Remember, Redis always looks at the data type stored at the key before it applies the command.

```
set list-four abc
rpush list-four a b c d
```
In this case, the key “list-four” is of data type String with the value “abc”. When RPUSH is executed, Redis first check that the key contains a List before it tries to add the values.

In this case, the key contains a String rather than a List, so therefore an Error is thrown.

The TYPE command can be used to inspect the data type stored at the Key.

