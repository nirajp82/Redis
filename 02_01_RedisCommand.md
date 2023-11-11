
## Note 1:
Remember, Redis always looks at the data type stored at the key before it applies the command. Commands are valid for specific data types. For example, SADD will only operate against a key with the Set datatype.

```
set list-four abc
rpush list-four a b c d
```
In this case, the key “list-four” is of data type String with the value “abc”. When RPUSH is executed, Redis first check that the key contains a List before it tries to add the values.

In this case, the key contains a String rather than a List, so therefore an Error is thrown.

The TYPE command can be used to inspect the data type stored at the Key.

## Note 2
Some commands will inspect the encoding of the value before they operate For example, INCR first determine if the key is a String data type. It then checks if the encoding is a number before the command executes.


