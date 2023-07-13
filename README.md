A kata in c#

Originally done in <https://coderpad.io>

# O-Complexity

ChatGPT had this to say about the complexity of the algorithm, which is interesting and would need validating properly but doesn't look too far out to me:


> The provided code is responsible for processing a file containing words and grouping them based on anagrams. Let's analyze the time complexity of each method:
> 
> 1. Run(string inputFile): This method reads the lines from the input file and calls ProcessWords method. Reading the lines from the file takes O(N) time complexity, where N is the number of lines in the file. Therefore, the time complexity of this method depends on the time complexity of ProcessWords.
> 
> 1. ProcessWords(IEnumerable<string> source, System.IO.TextWriter output): This method processes the words from the source collection in batches. It iterates over each word and performs some operations based on its length. Let's break down the time complexity:
>     - The foreach loop iterates over each word, which takes O(M) time, where M is the total number of words in the source collection.
>     - Inside the loop, adding a word to the words list takes O(1) time.
>     - When a new batch starts (a word with a different length is encountered), the previous batch is processed using the ProcessBatch method. This method has a time complexity of O(K), where K is the size of the batch (the number of words with the same length).
>     - Clearing the words list takes O(K) time.
>     - Finally, the last batch is processed outside the loop.
> 
> 1. Overall, the time complexity of ProcessWords can be approximated as O(M + K), where M is the total number of words and K represents the size of each batch.
> 
> 1. ProcessBatch(List<string> words, System.IO.TextWriter output): This method calls the GroupAnagrams method to group the words based on anagrams. It then iterates over the grouped anagrams and writes them to the output. The time complexity of this method depends on the time complexity of GroupAnagrams and the number of groups.
> 
> 1. GroupAnagrams(IList<string> input): This method uses LINQ's GroupBy to group the input words based on sorted versions of each word. The time complexity of GroupBy is O(N log N), where N is the number of input words. Therefore, the time complexity of GroupAnagrams is also O(N log N).
> 
> 1. SortString(string input): This method sorts the characters of the input string using Array.Sort. The time complexity of Array.Sort is O(K log K), where K is the length of the input string. Therefore, the time complexity of SortString is O(K log K).
> 
> In conclusion, the time complexity of the main processing logic in the provided code is approximately O(M + K + N log N + KG), where M is the total number of words, K is the size of each batch, N is the number of input words, and G is the number of groups of anagrams.
