****************
Free Text Search
****************

The free text input performs an handy quick meaning search using a consistent notation rule for expressing a phrase and logical operators.

The query string is parsed into a series of terms and operators. A term can be a single word — L0 or SLC — or a phrase, surrounded by double quotes — "Synthetic Aperture Radar" — which searches for all the words in the phrase, in the same order.

EOP Metadata fields
+++++++++++++++++++

A dataset is described by a set of fields. Some of them are accessible for specific filter:

+------------+--------+----------------------------------+
| Field Name | Type   | Description                      |
+============+========+==================================+
| isn        | string | instrument short name            |
+------------+--------+----------------------------------+
| ot         | string | orbit type                       |
+------------+--------+----------------------------------+
| pl         | string | processing level                 |
+------------+--------+----------------------------------+
| psn        | string | platform short name              |
+------------+--------+----------------------------------+
| psi        | string | platform serial identifier       |
+------------+--------+----------------------------------+
| pt         | string | product type                     |
+------------+--------+----------------------------------+
| sr         | string | sensor spectral range            |
+------------+--------+----------------------------------+
| st         | string | sensor Type                      |
+------------+--------+----------------------------------+
| summary    | string | abstract of the dataset          |
+------------+--------+----------------------------------+
| title      | string | title of the dataset             |
+------------+--------+----------------------------------+
| wl         | double | sensor wavelengths in nanometers |
+------------+--------+----------------------------------+

Field names
+++++++++++

All fields of the metadata of the dataset is searched for the search terms, but it is possible to specify other fields in the query syntax:

- where the track field is 15
::

	track:15

- where the swath field contains IW or EW. If you omit the OR operator the default operator will be used
::

	swath:(IW OR EW)

- where the instrument description field contains the exact phrase "Synthetic Aperture Radar"
::

	instrumentDescription:"Synthetic Aperture Radar"

- where any of the fields processingInformation.method, processingInformation.processorName or processingInformation.processingLevel contains L1 or Level 1 (note how we need to escape the * with a backslash):
::

	processingInformation.\*:(L1 "Level 1")

- where the field title has no value (or is missing):
::

_missing_:title

- where the field title has any non-null value:
::

_exists_:title

Wildcards
+++++++++

Wildcard searches can be run on individual terms, using ? to replace a single character, and * to replace zero or more characters:
::

	IW? S1*

Be aware that wildcard queries can use an enormous amount of memory and perform very badly — just think how many terms need to be queried to match the query string "a* b* c*".

.. warning::

Allowing a wildcard at the beginning of a word (eg "*ing") is particularly heavy, because all terms in the index need to be examined, just in case they match.


Wildcarded terms are not analyzed by default — they are lowercased but no further analysis is done, mainly because it is impossible to accurately analyze a word that is missing some of its letters. 

Regular expressions
+++++++++++++++++++

Regular expression patterns can be embedded in the query string by wrapping them in forward-slashes ("/"):
::

	parentIdentifier:/[EI]W_SLC__1SS.?/

The supported regular expression syntax is explained in :doc:`Regular expression syntax <regex>`.

.. WARNING::

A query string such as the following would force Elasticsearch to visit every term in the index:
::

	/.*n/
Use with caution!


Fuzziness
+++++++++

We can search for terms that are similar to, but not exactly like our search terms, using the “fuzzy” operator:
::

	sent~ rdar~

This uses the Damerau-Levenshtein distance to find all terms with a maximum of two changes, where a change is the insertion, deletion or substitution of a single character, or transposition of two adjacent characters.

The default edit distance is 2, but an edit distance of 1 should be sufficient to catch 80% of all human misspellings. It can be specified as:
:: 

	quikc~1

Ranges
++++++

Ranges can be specified for date, numeric or string fields. Inclusive ranges are specified with square brackets [min TO max] and exclusive ranges with curly brackets {min TO max}.

All days in 2012:
::

	startDate:[2012-01-01 TO 2012-12-31]

Track 1..5
::

	track:[1 TO 5]

Topic categories between alpha and omega, excluding alpha and omega:
::

	tc:{alpha TO omega}

Processing Level from L1 upwards
::

	pl:[L1 TO *]

modified before 2012
::

	modified:{* TO 2012-01-01}

Curly and square brackets can be combined:

Numbers from 1 up to but not including 5
::

	track:[1 TO 5}

Ranges with one side unbounded can use the following syntax:
::

	orbitNumber:>10
	orbitNumber:>=10
	orbitNumber:<10
	orbitNumber:<=10

Note
To combine an upper and lower bound with the simplified syntax, you would need to join two clauses with an AND operator:
::

	orbitNumber:(>=10 AND <20)
	orbitNumber:(+>=10 +<20)

The parsing of ranges in query strings can be complex and error prone. It is much more reliable to use an explicit range filter.

Boosting
++++++++

Use the boost operator ^ to make one term more relevant than another. For instance, if we want to find all datasets in dual polarisation, but we are especially interested in dual polarisation in IW swath:
::

	som:IW_DP^2 pm:D

The default boost value is 1, but can be any positive floating point number. Boosts between 0 and 1 reduce relevance.

Boosts can also be applied to phrases or to groups:
::

	"Synthetic Aperture Radar"^2   (IW_DP SAR)^4

Boolean operators
+++++++++++++++++

By default, all terms are optional, as long as one term matches. A search for sar msi atsr will find any document that contains one or more of sar or msi or atsr. We have already discussed the default operator above which allows you to force all terms to be required, but there are also boolean operators which can be used in the query string itself to provide more control.

The preferred operators are + (this term must be present) and - (this term must not be present). All other terms are optional. For example, this query:
::

	S1A SAR +IW -EW

states that:

IW must be present
EW must not be present
S1A and SAR are optional — their presence increases the relevance
The familiar operators AND, OR and NOT (also written &&, || and !) are also supported. However, the effects of these operators can be more complicated than is obvious at first glance. NOT takes precedence over AND, which takes precedence over OR. While the + and - only affect the term to the right of the operator, AND and OR can affect the terms to the left and right.

Rewriting the above query using AND, OR and NOT demonstrates the complexity:
::

	S1A OR SAR AND IW AND NOT EW

This is incorrect, because SAR is now a required term.
::

	(S1A OR SAR) AND IW AND NOT EW

This is incorrect because at least one of S1A or SAR is now required and the search for those terms would be scored differently from the original query.
::

	((S1A AND IW) OR (SAR AND IW) OR IW) AND NOT EW

This form now replicates the logic from the original query correctly, but the relevance scoring bares little resemblance to the original.

Grouping
++++++++

Multiple terms or clauses can be grouped together with parentheses, to form sub-queries:
::

	(S1A OR SAR) AND IW

Groups can be used to target a particular field, or to boost the result of a sub-query:
::

	status:(archived OR planned) at:(nominal calibration)^2

Reserved characters
+++++++++++++++++++

If you need to use any of the characters which function as operators in your query itself (and not as operators), then you should escape them with a leading backslash. For instance, to search for (1+1)=2, you would need to write your query as \(1\+1\)\=2.

The reserved characters are: 
::

	+ - = && || > < ! ( ) { } [ ] ^ " ~ * ? : \ /

Failing to escape these special characters correctly could lead to a syntax error which prevents your query from running.