### 순서를 지켜야 하는 Task 작업을 위한 라이브러리
윈도우 어플리케이션 프로젝트를 진행할 때 버튼 등의 반응에 대해 UI 쓰레드를 BLOCK 하지 않고 비동기적으로 작업해야하는 경우가 많다.
이와 동시에 비동기적으로 진행되는 작업이 순서를 지켜야하는 경우가 종종 발생한다.
이런 상황을 위한 Queue 객체를 정의한 라이브러리이다.

```
/// <summary>
/// Task 삽입 (큐에 즉시 Task가 등록되고 순서에서 맞춰서 즉시 실행된다)
/// </summary>
/// <param name="task">Task, Task<Task>(async)</param>
/// <returns>대기 Task</returns>
public Task Enqueue(Task task)
```

```
/// <summary>
/// Task<T> 삽입 (큐에 즉시 Task가 등록되고 순서에서 맞춰서 즉시 실행된다)
/// </summary>
/// <typeparam name="TResult"></typeparam>
/// <param name="task">Task, Task<Task>(async)</param>
/// <returns>대기 Task</returns>
public Task<TResult> Enqueue<TResult>(Task task)
```

### ex) 다양한 형태의 Task를 이용한 예제
```
TaskQueue queue = new TaskQueue();

double result = 0;

public Form1()
{
    InitializeComponent();

    button1_Click(null, null);
    button2_Click(null, null);
    button3_Click(null, null);
    button4_Click(null, null);
}

private async void button1_Click(object sender, EventArgs e)
{
    Task waiter = queue.Enqueue(new Task(() => {
        Task.Delay(500).Wait();
        Console.WriteLine("1 START");
        Task.Delay(500).Wait();
        result += 1;
    }));
    await waiter;
    Console.WriteLine("1 END");
}

private async void button2_Click(object sender, EventArgs e)
{
    Task waiter = queue.Enqueue(new Task<Task>(async () =>
    {
        await Task.Delay(500);
        Console.WriteLine("2 START");
        await Task.Delay(500);
        result *= 2;
    }));
    await waiter;
    Console.WriteLine("2 END");
}

private async void button3_Click(object sender, EventArgs e)
{
    Task<double> waiter = queue.Enqueue<double>(new Task<double>(() =>
    {
        Task.Delay(500).Wait();
        Console.WriteLine("3 START");
        Task.Delay(500).Wait();
        result -= 3;
        return result;
    }));
    Console.WriteLine("3 END / " + await waiter);
}

private async void button4_Click(object sender, EventArgs e)
{
    Task<double> waiter = queue.Enqueue<double>(new Task<Task<double>>(async () =>
    {
        await Task.Delay(500);
        Console.WriteLine("4 START");
        await Task.Delay(500);
        result /= 4;
        return result;
    }));
    Console.WriteLine("4 END / " + await waiter);
}
```

### 결과
```
1 START
1 END
2 START
2 END
3 START
3 END / -1
4 START
4 END / -0.25
```
