﻿//inspired by, and init and tail algorithms from,  http://fssnip.net/dJ

//pattern discriminators Cons, Snoc, and Nil

namespace FSharpx.DataStructures

open System.Collections
open System.Collections.Generic
open ListHelpr

type BatchedDeque<'a> (front, rBack) = 
  
    static member internal Empty() = BatchedDeque(List.Empty, List.Empty)

    static member internal OfCatLists (xs : 'a list) (ys : 'a list) =
        new BatchedDeque<'a>(xs, (List.rev ys))

    static member internal OfSeq (xs:seq<'a>) = 
        new BatchedDeque<'a>((List.ofSeq xs), [])

    static member internal Singleton x = BatchedDeque([x], List.Empty)

    ///returns a new deque with the element added to the beginning
    member this.Cons x =  BatchedDeque(x::front, rBack)
           
    ///returns the first element 
    member this.Head =
        match front, rBack with
        | [], [] -> raise Exceptions.Empty
        | hd::tl, _ -> hd
        | [], xs -> List.rev xs |> List.head

    ///returns option first element
    member this.TryGetHead =
        match front, rBack with
        | [], [] -> None
        | hd::tl, _ -> Some(hd)
        | [], xs -> 
            let x = List.rev xs |> List.head 
            Some(x)

    ///returns a new deque of the elements before the last element
    member this.Init = 
        match front, rBack with 
        | [],  [] -> raise Exceptions.Empty
        | _ , x::xs -> BatchedDeque(front, xs)
        | _ ,   [] ->       //splits front in two, favoring frontbot for odd length
            let _, fronttop, frontbot = List.fold(fun (i, reartop, rearbot) e -> 
                if i < rBack.Length /2
                then (i+1, e::reartop, rearbot)
                else (i+1, reartop, e::rearbot)) (0,[],[]) front
            let front', rear' = fronttop |> List.rev , frontbot
            BatchedDeque(front', rear'.Tail) 
            
    ///returns a new deque of the elements before the last element
    member this.TryGetInit = 
        match front, rBack with 
        | [],  [] -> None
        | _ , x::xs -> Some(BatchedDeque(front, xs))
        | _ ,   [] ->       //splits front in two, favoring frontbot for odd length
            let _, fronttop, frontbot = List.fold(fun (i, reartop, rearbot) e -> 
                if i < rBack.Length /2
                then (i+1, e::reartop, rearbot)
                else (i+1, reartop, e::rearbot)) (0,[],[]) front
            let front', rear' = fronttop |> List.rev , frontbot
            Some(BatchedDeque(front', rear'.Tail)) 
          
    ///returns true if the deque has no elements  
    member this.IsEmpty =  
        match front, rBack with
        | [], [] -> true | _ -> false

    ///returns the last element
    member this.Last = 
        match front, rBack with
        | [], [] -> raise Exceptions.Empty
        | xs, [] -> List.rev xs |> List.head
        | _, hd::tl -> hd

    ///returns option last element
    member this.TryGetLast = 
        match front, rBack with
        | [], [] -> None
        | xs, [] -> Some(List.rev xs |> List.head)
        | _, hd::tl -> Some(hd)

    ///returns the count of elememts
    member this.Length = front.Length + rBack.Length

    ///returns element by index
    member this.Lookup (i:int) =
        match (List.length front), front, (List.length rBack), rBack with
        | lenF, front, lenR, rear when i > (lenF + lenR - 1) -> raise Exceptions.OutOfBounds
        | lenF, front, lenR, rear when i < lenF -> 
            let rec loopF = function 
                | xs, i'  when i' = 0 -> List.head xs
                | xs, i' -> loopF ((List.tail xs), (i' - 1))
            loopF (front, i)
        | lenF, front, lenR, rear ->  
            let rec loopF = function 
                | xs, i'  when i' = 0 -> List.head xs
                | xs, i' -> loopF ((List.tail xs), (i' - 1))
            loopF (rear, ((lenR - (i - lenF)) - 1))

    ///returns option element by index
    member this.TryLookup (i:int) =
        match (List.length front), front, (List.length rBack), rBack with
        | lenF, front, lenR, rear when i > (lenF + lenR - 1) -> None
        | lenF, front, lenR, rear when i < lenF -> 
            let rec loopF = function 
                | xs, i'  when i' = 0 -> Some(List.head xs)
                | xs, i' -> loopF ((List.tail xs), (i' - 1))
            loopF (front, i)
        | lenF, front, lenR, rear ->  
            let rec loopF = function 
                | xs, i'  when i' = 0 -> Some(List.head xs)
                | xs, i' -> loopF ((List.tail xs), (i' - 1))
            loopF (rear, ((lenR - (i - lenF)) - 1))

    ///returns deque with element removed by index
    member this.Remove (i:int) =
        match (List.length front), front, (List.length rBack), rBack with
        | lenF, front, lenR, rear when i > (lenF + lenR - 1) -> raise Exceptions.OutOfBounds
        | lenF, front, lenR, rear when i < lenF -> 
            let newFront = 
                if (i = 0) then List.tail front
                else 
                    let left, right = loop2Array (Array.create i (List.head front)) front (i-1)    
                    loopFromArray ((Seq.length left) - 1) left right 0

            (new BatchedDeque<'a>(newFront, rear))

        | lenF, front, lenR, rear ->  
            let n = lenR - (i - lenF) - 1
            let newRear = 
                if (n = 0) then List.tail rear
                else 
                    let left, right = loop2Array (Array.create n (List.head rear)) rear (n-1) 
                    loopFromArray ((Seq.length left) - 1) left right 0

            (new BatchedDeque<'a>(front, newRear))

    ///returns option deque with element removed by index
    member this.TryRemove (i:int) =
        match (List.length front), front, (List.length rBack), rBack with
        | lenF, front, lenR, rear when i > (lenF + lenR - 1) -> None
        | lenF, front, lenR, rear when i < lenF -> 
            let newFront = 
                if (i = 0) then List.tail front
                else 
                    let left, right = loop2Array (Array.create i (List.head front)) front (i-1) 
                    loopFromArray ((Seq.length left) - 1) left right 0

            Some((new BatchedDeque<'a>(newFront, rear)))

        | lenF, front, lenR, rear ->  
            let n = lenR - (i - lenF) - 1
            let newRear = 
                if (n = 0) then List.tail rear
                else 
                    let left, right = loop2Array (Array.create n (List.head rear)) rear (n-1) 
                    loopFromArray ((Seq.length left) - 1) left right 0

            Some((new BatchedDeque<'a>(front, newRear)))

    ///returns deque reversed
    member this.Rev = 
        (new BatchedDeque<'a>(rBack, front))

    ///returns a new deque with the element added to the end
    member this.Snoc x = BatchedDeque(front, x::rBack)

    ///returns a new deque of the elements trailing the first element
    member this.Tail =
        match front, rBack with
        | [],  [] -> raise Exceptions.Empty
        | x::xs,  _ ->  BatchedDeque(xs, rBack)
        | _,  _ ->      //splits rear in two, favoring rearbot for odd length
            let _, reartop, rearbot = 
                List.fold(fun (i, reartop, rearbot) e -> 
                    if i < rBack.Length / 2 
                    then (i+1, e::reartop, rearbot)
                    else (i+1, reartop, e::rearbot)) (0,[],[]) rBack
            let rear', front' = reartop |> List.rev, rearbot 
            BatchedDeque(front'.Tail, rear')

    ///returns option deque of the elements trailing the first element
    member this.TryGetTail =
        match front, rBack with
        | [],  [] -> None
        | x::xs,  _ ->  Some(BatchedDeque(xs, rBack))
        | _,  _ ->      //splits rear in two, favoring rearbot for odd length
            let _, reartop, rearbot = 
                List.fold(fun (i, reartop, rearbot) e -> 
                    if i < rBack.Length / 2 
                    then (i+1, e::reartop, rearbot)
                    else (i+1, reartop, e::rearbot)) (0,[],[]) rBack
            let rear', front' = reartop |> List.rev, rearbot 
            Some(BatchedDeque(front'.Tail, rear'))

    ///returns the first element and tail
    member this.Uncons =  
        match front, rBack with
        | [], [] -> raise Exceptions.Empty
        | _, _ -> this.Head, this.Tail

    ///returns option first element and tail
    member this.TryUncons =  
        match front, rBack with
        | [], [] -> None
        | _, _ -> Some(this.Head, this.Tail)

    ///returns init and the last element
    member this.Unsnoc =  
        match front, rBack with
        | [], [] -> raise Exceptions.Empty
        | _, _ -> this.Init, this.Last

    ///returns option init and the last element
    member this.TryUnsnoc =  
        match front, rBack with
        | [], [] -> None
        | _, _ -> Some(this.Init, this.Last)
          
    ///returns deque with element updated by index
    member this.Update (i:int) (y: 'a) =
        match (List.length front), front, (List.length rBack), rBack with
        | lenF, front, lenR, rear when i > (lenF + lenR - 1) -> raise Exceptions.OutOfBounds
        | lenF, front, lenR, rear when i < lenF -> 
            let newFront = 
                if (i = 0) then y::(List.tail front)
                else 
                    let left, right = loop2Array (Array.create i (List.head front)) front (i-1) 
                    loopFromArray ((Seq.length left) - 1) left (y::right) 0

            new BatchedDeque<'a>(newFront, rear)

        | lenF, front, lenR, rear ->  
            let n = lenR - (i - lenF) - 1
            let newRear = 
                if (n = 0) then y::(List.tail rear)
                else 
                    let left, right = loop2Array (Array.create n (List.head rear)) rear (n-1) 
                    loopFromArray ((Seq.length left) - 1) left (y::right) 0
        
            new BatchedDeque<'a>(front, newRear)

    ///returns option deque with element updated by index
    member this.TryUpdate (i:int) (y: 'a) =
        match (List.length front), front, (List.length rBack), rBack with
        | lenF, front, lenR, rear when i > (lenF + lenR - 1) -> None
        | lenF, front, lenR, rear when i < lenF -> 
            let newFront = 
                if (i = 0) then y::(List.tail front)
                else 
                    let left, right = loop2Array (Array.create i (List.head front)) front (i-1) 
                    loopFromArray ((Seq.length left) - 1) left (y::right) 0

            Some((new BatchedDeque<'a>(newFront, rear)))

        | lenF, front, lenR, rear ->  
            let n = lenR - (i - lenF) - 1
            let newRear = 
                if (n = 0) then y::(List.tail rear)
                else 
                    let left, right = loop2Array (Array.create n (List.head rear)) rear (n-1)
                    loopFromArray ((Seq.length left) - 1) left (y::right) 0
        
            Some((new BatchedDeque<'a>(front, newRear)))

    with
    interface IDeque<'a> with

        member this.Cons x = this.Cons x :> _

        member this.Count = this.Length

        member this.Head = this.Head

        member this.TryGetHead = this.TryGetHead

        member this.Init = this.Init :> _

        member this.TryGetInit = Some(this.TryGetInit.Value :> _)

        member this.IsEmpty = this.IsEmpty

        member this.Last = this.Last

        member this.TryGetLast = this.TryGetLast

        member this.Length = this.Length

        member this.Lookup i = this.Lookup i

        member this.TryLookup i = this.TryLookup i

        member this.Remove i = this.Remove i :> _

        member this.TryRemove i = 
            match this.TryRemove i with
            | None -> None
            | Some(q) -> Some(q :> _)

        member this.Rev = this.Rev :> _

        member this.Snoc x = this.Snoc x :> _

        member this.Tail = this.Tail :> _

        member this.TryGetTail = 
            match this.TryGetTail with
            | None -> None
            | Some(q) -> Some(q :> _)

        member this.Uncons = 
            let x, xs = this.Uncons 
            x, xs :> _

        member this.TryUncons = 
            match this.TryUncons with
            | None -> None
            | Some(x, q) -> Some(x, q :> _)

        member this.Unsnoc = 
            let xs, x = this.Unsnoc 
            xs :> _, x

        member this.TryUnsnoc = 
            match this.TryUnsnoc with
            | None -> None
            | Some(q, x) -> Some(q :> _, x)

        member this.Update i y  = this.Update i y :> _

        member this.TryUpdate i y  =
            match this.TryUpdate i y with
            | None -> None
            | Some(q) -> Some(q :> _)

    interface IEnumerable<'a> with

        member this.GetEnumerator() = 
            let e = seq {
                  yield! front
                  yield! (List.rev rBack)}
            e.GetEnumerator()

        member this.GetEnumerator() = (this :> _ seq).GetEnumerator() :> IEnumerator

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module BatchedDeque =

    //pattern discriminator

    let (|Cons|Nil|) (q : BatchedDeque<'a>) = match q.TryUncons with Some(a,b) -> Cons(a,b) | None -> Nil

    let (|Snoc|Nil|) (q : BatchedDeque<'a>) = match q.TryUnsnoc with Some(a,b) -> Snoc(a,b) | None -> Nil

    ///returns a new deque with the element added to the beginning
    let inline cons (x : 'a) (q : BatchedDeque<'a>) = q.Cons x 

    //returns deque of no elements
    let empty() = BatchedDeque.Empty()

    ///returns the first element
    let inline head (q : BatchedDeque<'a>) = q.Head

    ///returns option first element
    let inline tryGetHead (q : BatchedDeque<'a>) = q.TryGetHead

    ///returns a new deque of the elements before the last element
    let inline init (q : BatchedDeque<'a>) = q.Init 

    ///returns option deque of the elements before the last element
    let inline tryGetInit (q : BatchedDeque<'a>) = q.TryGetInit 

    ///returns true if the deque has no elements
    let inline isEmpty (q : BatchedDeque<'a>) = q.IsEmpty

    ///returns the last element
    let inline last (q : BatchedDeque<'a>) = q.Last

    ///returns option last element
    let inline tryGetLast (q : BatchedDeque<'a>) = q.TryGetLast

    ///returns the count of elememts
    let inline length (q : BatchedDeque<'a>) = q.Length

    ///returns element by index
    let inline lookup i (q : BatchedDeque<'a>) = q.Lookup i

    ///returns option element by index
    let inline tryLookup i (q : BatchedDeque<'a>) = q.TryLookup i

    ///returns a deque of the two lists concatenated
    let ofCatLists xs ys = BatchedDeque.OfCatLists xs ys

    ///returns a deque of the seq
    let ofSeq xs = BatchedDeque.OfSeq xs

    ///returns deque with element removed by index
    let inline remove i (q : BatchedDeque<'a>) = q.Remove i

    ///returns option deque with element removed by index
    let inline tryRemove i (q : BatchedDeque<'a>) = q.TryRemove i

    ///returns deque reversed
    let inline rev (q : BatchedDeque<'a>) = q.Rev

    ///returns a deque of one element
    let singleton x = BatchedDeque.Singleton x

    ///returns a new deque with the element added to the end
    let inline snoc (x : 'a) (q : BatchedDeque<'a>) = (q.Snoc x) 

    ///returns a new deque of the elements trailing the first element
    let inline tail (q : BatchedDeque<'a>) = q.Tail 

    ///returns option deque of the elements trailing the first element
    let inline tryGetTail (q : BatchedDeque<'a>) = q.TryGetTail 

    ///returns the first element and tail
    let inline uncons (q : BatchedDeque<'a>) = q.Uncons

    ///returns option first element and tail
    let inline tryUncons (q : BatchedDeque<'a>) = q.TryUncons

    ///returns init and the last element
    let inline unsnoc (q : BatchedDeque<'a>) = q.Unsnoc

    ///returns option init and the last element
    let inline tryUnsnoc (q : BatchedDeque<'a>) = q.TryUnsnoc

    ///returns deque with element updated by index
    let inline update i y (q : BatchedDeque<'a>) = q.Update i y

    ///returns option deque with element updated by index
    let inline tryUpdate i y (q : BatchedDeque<'a>) = q.TryUpdate i y