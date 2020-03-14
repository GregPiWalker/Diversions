The following code is an excerpt of the demo application.  It illustrates how
this library can be used easily in an intuitive fashion.  The sample configures
the library to execute event handlers on the UI thread, which is useful for
View DataBindings that target observable properties or collections on a 
business model (whether directly, or indirectly via a ViewModel).

While this sample declares an event handler to be dispatched to the UI thread,
a common use case is a View binding directly to an ObservableCollection.
In this case, there is no opportunity to explicitly define the thread context.
By using the provided MarshallingObservableCollection class, you can use the
same collection to supply notifications to DataBindings on the UI thread
as well as notifications to the business layer without involving the UI thread.


Example code:
-----------------------------------------------------------------------------------

/**
* Configure the default marshalling behavior to dispatch work to the main thread.
* This will cause every event handler that does not explicitly define a marshal
* option to use the default option.
*/
static Model()
{
    ThreadedHandlerAttribute.DefaultThreadOption = MarshalOption.Dispatcher;
}

/**
* Declare an event that is backed by a MarshallingDelegate.
*/
public event EventHandler<EventArgs> Event
{
    add { _eventDelegate.Add(value); }
    remove { _eventDelegate.Remove(value); }
}

/**
* Hook event handlers up to the event, as per usual, then
* raise the event without concern for the threading model 
* required by downstream consumers.
*/
public Model()
{
    Event += HandleEventOnUiThread;
    Event += HandleEventOnTask;
    Event += HandleEventOnCurrentThread;

    // Raise the event once by invoking the MarshallingDelegate.  All three handlers
    // are executed, each with their own threading context.
    _eventDelegate.Invoke(this, null);
}

private void HandleEventOnUiThread(object sender, EventArgs args)
{
    // Everything in here is executed on the Application's main thread via the Dispatcher.
    // Since no option was defined, this method implicitly uses the default marshalling option.
}

[ThreadedHandler(MarshalOption.RunTask)]
private void HandleEventOnTask(object sender, EventArgs args)
{
    // Everything in here is executed on a Task.
}

[ThreadedHandler(MarshalOption.CurrentThread)]
private void HandleEventOnCurrentThread(object sender, EventArgs args)
{
    // Everything in here is executed on the same thread that raised the source event.
}